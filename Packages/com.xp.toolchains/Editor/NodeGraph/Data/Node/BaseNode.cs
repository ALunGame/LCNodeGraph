using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XPToolchains.Json;

namespace XPToolchains.NodeGraph
{
    //自定义端口行为委托
    public delegate IEnumerable<PortData> CustomPortBehaviorDelegate(List<BaseEdge> edges);

    public class BaseNode
    {
        //唯一标识
        public string id;

        //显示区域
        public Rect position;
        //是否展开
        public bool expanded;

        //视图
        [JsonIgnore]
        public BaseGraph graph;

        //输入端口
        [JsonIgnore]
        public NodeInputPortContainer inputPorts;

        //输出端口
        [JsonIgnore]
        public NodeOutputPortContainer outputPorts;

        public BaseNode()
        {
            inputPorts = new NodeInputPortContainer(this);
            outputPorts = new NodeOutputPortContainer(this);
            InitInOutDatas();
        }

        #region 显示属性

        //名称
        public virtual string name => GetType().Name;

        //颜色
        public virtual Color color => Color.clear;

        //样式
        public virtual string layoutStyle => string.Empty;

        //是否需要在检查器中可见
        public virtual bool needsInspector => _needsInspector;
        [NonSerialized]
        bool _needsInspector = false;

        //是否可以删除
        public virtual bool deletable => true;

        //只有当鼠标在节点上时才显示节点
        public virtual bool showControlsOnHover => false;

        #endregion

        #region 触发事件

        //当边缘连接到节点上时触发
        public event Action<BaseEdge> onAfterEdgeConnected;
        //在节点上的边缘断开后触发
        public event Action<BaseEdge> onAfterEdgeDisconnected;
        //在单个/端口列表更新后触发，参数是字段名
        public event Action<string> onPortsUpdated;

        #endregion

        #region 字段信息
        internal class NodeFieldInformation
        {
            public string name;
            public string fieldName;
            public FieldInfo info;
            public bool input;
            public bool isMultiple;
            public string tooltip;
            public CustomPortBehaviorDelegate behavior;
            public bool vertical;

            public NodeFieldInformation(FieldInfo info, string name, bool input, bool isMultiple, string tooltip, bool vertical, CustomPortBehaviorDelegate behavior)
            {
                this.input = input;
                this.isMultiple = isMultiple;
                this.info = info;
                this.name = name;
                this.fieldName = info.Name;
                this.behavior = behavior;
                this.tooltip = tooltip;
                this.vertical = vertical;
            }
        }
        [NonSerialized]
        internal Dictionary<string, NodeFieldInformation> nodeFields = new Dictionary<string, NodeFieldInformation>();
        #endregion

        #region 端口信息

        struct PortUpdate
        {
            public List<string> fieldNames;
            public BaseNode node;

            //拆解
            public void Deconstruct(out List<string> fieldNames, out BaseNode node)
            {
                fieldNames = this.fieldNames;
                node = this.node;
            }
        }

        Stack<PortUpdate> fieldsToUpdate = new Stack<PortUpdate>();
        //为了避免更新俩次
        HashSet<PortUpdate> updatedFields = new HashSet<PortUpdate>();

        #endregion

        #region Init

        //初始化
        public void Init(BaseGraph graph)
        {
            this.graph = graph;
            inputPorts.Clear();
            outputPorts.Clear();

            InitPorts();
            ExceptionToLog.Call(() => Enable());
            UpdateAllPorts();
        }

        //初始化端口
        public virtual void InitPorts()
        {
            
            foreach (var key in nodeFields.Values.Select(k => k.info))
            {
                var nodeField = nodeFields[key.Name];
                // 创建一个端口
                AddPort(nodeField.input, nodeField.fieldName, new PortData { acceptMultipleEdges = nodeField.isMultiple, displayName = nodeField.name, tooltip = nodeField.tooltip, vertical = nodeField.vertical });
            }
        }

        //初始化输入输出参数信息
        void InitInOutDatas()
        {
            var fields = GetNodeFields();
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //生成输入输出参数信息
            foreach (var field in fields)
            {
                var inputAttribute = field.GetCustomAttribute<InputAttribute>();
                var outputAttribute = field.GetCustomAttribute<OutputAttribute>();
                var tooltipAttribute = field.GetCustomAttribute<TooltipAttribute>();
                var vertical = field.GetCustomAttribute<VerticalAttribute>();

                //多个
                bool isMultiple = false;
                bool input = false;
                string name = field.Name;
                string tooltip = null;

                if (inputAttribute == null && outputAttribute == null)
                    continue;

                //检查字段是否为集合类型
                isMultiple = (inputAttribute != null) ? inputAttribute.allowMultiple : (outputAttribute.allowMultiple);

                input = inputAttribute != null;
                tooltip = tooltipAttribute?.tooltip;

                if (!String.IsNullOrEmpty(inputAttribute?.name))
                    name = inputAttribute.name;
                if (!String.IsNullOrEmpty(outputAttribute?.name))
                    name = outputAttribute.name;

                nodeFields[field.Name] = new NodeFieldInformation(field, name, input, isMultiple, tooltip, vertical != null, null);
            }
        }

        #endregion

        #region Check

        //检测是否是输入
        public bool IsFieldInput(string fieldName) => nodeFields[fieldName].input;

        #endregion

        #region 更新端口数据

        //更新所有端口
        public bool UpdateAllPorts()
        {
            bool changed = false;

            foreach (var field in nodeFields)
                changed |= UpdatePortsForField(field.Value.fieldName);

            return changed;
        }

        //更新与一个c#属性字段和图中所有连接节点相关的端口
        public bool UpdatePortsForField(string fieldName)
        {
            bool changed = false;

            fieldsToUpdate.Clear();
            updatedFields.Clear();

            fieldsToUpdate.Push(new PortUpdate { fieldNames = new List<string>() { fieldName }, node = this });

            while (fieldsToUpdate.Count != 0)
            {
                var (fields, node) = fieldsToUpdate.Pop();

                // 避免更新俩次
                if (updatedFields.Any((t) => t.node == node && fields.SequenceEqual(t.fieldNames)))
                    continue;

                updatedFields.Add(new PortUpdate { fieldNames = fields, node = node });
                foreach (var field in fields)
                {
                    if (node.UpdatePortsForFieldLocal(field))
                    {
                        changed = true;
                    }
                }
            }

            return changed;
        }

        //更新节点所有端口，但不更新已连接端口
        public bool UpdateAllPortsLocal()
        {
            bool changed = false;

            foreach (var field in nodeFields)
                changed |= UpdatePortsForFieldLocal(field.Value.fieldName);

            return changed;
        }

        //更新与一个c#属性字段相关的端口(仅针对这个节点)
        public bool UpdatePortsForFieldLocal(string fieldName)
        {
            bool changed = false;

            if (!nodeFields.ContainsKey(fieldName))
                return false;

            NodeFieldInformation fieldInfo = nodeFields[fieldName];

            if (fieldInfo.behavior == null)
                return false;

            //更新后的端口数据
            List<string> finalPorts = new List<string>();

            NodePortContainer portCollection = fieldInfo.input ? (NodePortContainer)inputPorts : outputPorts;

            // 收集连接此字段的端口
            var nodePorts = portCollection.Where(p => p.fieldName == fieldName);
            if (nodePorts == null)
                return changed;

            // 收集端口的边缘
            var edges = nodePorts.SelectMany(n => n.GetEdges()).ToList();

            // 自定义行为
            if (fieldInfo.behavior != null)
            {
                foreach (var portData in fieldInfo.behavior(edges))
                    AddPortData(portData);
            }

            //添加端口数据
            void AddPortData(PortData portData)
            {
                var port = nodePorts.FirstOrDefault(n => n.portData.id == portData.id);
                if (port == null)
                {
                    AddPort(fieldInfo.input, fieldName, portData);
                    changed = true;
                }
                else
                {
                    // 如果端口类型不可以连接，断开连接到该端口的所有边缘
                    if (!BaseGraph.TypesAreConnectable(port.portData.displayType, portData.displayType))
                    {
                        foreach (var edge in port.GetEdges().ToList())
                            graph.DisconnectEdge(edge.id);
                    }

                    // 修补端口数据
                    if (port.portData != portData)
                    {
                        port.portData.CopyFrom(portData);
                        changed = true;
                    }
                }

                finalPorts.Add(portData.id);
            }

            // 清理
            var currentPortsCopy = nodePorts.ToList();
            foreach (var currentPort in currentPortsCopy)
            {
                if (!finalPorts.Any(id => id == currentPort.portData.id))
                {
                    RemovePort(fieldInfo.input, currentPort);
                    changed = true;
                }
            }

            // 端口排序
            portCollection.Sort((p1, p2) =>
            {
                int p1Index = finalPorts.FindIndex(id => p1.portData.id == id);
                int p2Index = finalPorts.FindIndex(id => p2.portData.id == id);

                if (p1Index == -1 || p2Index == -1)
                    return 0;

                return p1Index.CompareTo(p2Index);
            });

            onPortsUpdated?.Invoke(fieldName);

            return changed;
        }

        #endregion

        #region 子类重写函数

        //启用节点调用
        protected virtual void Enable() { }

        //在禁用节点时调用
        protected virtual void Disable() { }

        //删除节点时调用
        protected virtual void Destroy() { }

        //创建节点Id
        public virtual void CreatedNodeId() => graph.createNodeIdAction(graph, this);

        //是否可以重置参数
        protected virtual bool CanResetPort(NodePort port) => true;

        //获得类字段
        public virtual FieldInfo[] GetNodeFields()
            => GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        #endregion

        #region 处理事件

        //节点禁用
        internal void DisableInternal() => ExceptionToLog.Call(() => Disable());

        //节点销毁
        internal void DestroyInternal() => ExceptionToLog.Call(() => Destroy());

        //当边缘连接时
        public void OnEdgeConnected(BaseEdge edge)
        {
            //保存边缘
            bool input = edge.inputNode == this;
            NodePortContainer portCollection = (input) ? (NodePortContainer)inputPorts : outputPorts;
            portCollection.Add(edge);

            //更新端口
            UpdateAllPorts();

            //发送事件
            onAfterEdgeConnected?.Invoke(edge);
        }

        //当边缘断开连接时
        public void OnEdgeDisconnected(BaseEdge edge)
        {
            if (edge == null)
                return;

            //删除边缘
            bool input = edge.inputNode == this;
            NodePortContainer portCollection = (input) ? (NodePortContainer)inputPorts : outputPorts;
            portCollection.Remove(edge);

            //重置输入参数
            bool haveConnectedEdges = edge.inputNode.inputPorts.Where(p => p.fieldName == edge.inputFieldName).Any(p => p.GetEdges().Count != 0);
            if (edge.inputNode == this && !haveConnectedEdges && CanResetPort(edge.inputPort))
                edge.inputPort?.ResetToDefault();

            //更新端口
            UpdateAllPorts();

            //发送事件
            onAfterEdgeDisconnected?.Invoke(edge);
        }

        #endregion

        #region Public

        public IEnumerable<BaseNode> GetOutputNodes()
        {
            foreach (var port in outputPorts)
                foreach (var edge in port.GetEdges())
                    yield return edge.inputNode;
        }

        public void AddPort(bool input, string fieldName, PortData portData)
        {
            if (portData.displayType == null)
                portData.displayType = nodeFields[fieldName].info.FieldType;

            if (input)
                inputPorts.Add(new NodePort(this, fieldName, portData));
            else
                outputPorts.Add(new NodePort(this, fieldName, portData));
        }

        public NodePort GetPort(string fieldName, string id)
        {
            return inputPorts.Concat(outputPorts).FirstOrDefault(p =>
            {
                var bothNull = String.IsNullOrEmpty(id) && String.IsNullOrEmpty(p.portData.id);
                return p.fieldName == fieldName && (bothNull || id == p.portData.id);
            });
        }

        public void RemovePort(bool input, NodePort port)
        {
            if (input)
                inputPorts.Remove(port);
            else
                outputPorts.Remove(port);
        }

        public void RemovePort(bool input, string fieldName)
        {
            if (input)
                inputPorts.RemoveAll(p => p.fieldName == fieldName);
            else
                outputPorts.RemoveAll(p => p.fieldName == fieldName);
        }

        public IEnumerable<NodePort> GetAllPorts()
        {
            foreach (var port in inputPorts)
                yield return port;
            foreach (var port in outputPorts)
                yield return port;
        }

        public IEnumerable<BaseEdge> GetAllEdges()
        {
            foreach (var port in GetAllPorts())
                foreach (var edge in port.GetEdges())
                    yield return edge;
        }

        #endregion

        #region 静态方法

        public static T CreateFromType<T>(Vector2 position, BaseGraph graph) where T : BaseNode
        {
            return CreateFromType(typeof(T), position, graph) as T;
        }

        public static BaseNode CreateFromType(Type nodeType, Vector2 position, BaseGraph graph)
        {
            if (!nodeType.IsSubclassOf(typeof(BaseNode)))
                return null;

            var node = Activator.CreateInstance(nodeType) as BaseNode;

            node.position = new Rect(position, new Vector2(100, 100));
            node.graph = graph;

            ExceptionToLog.Call(() => node.CreatedNodeId());

            return node;
        }

        #endregion

    }
}
