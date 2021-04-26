using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XPToolchains.Json;

namespace XPToolchains.NodeGraph
{
    public class GraphChanges
    {
        public BaseEdge removedEdge;
        public BaseEdge addedEdge;

        public BaseNode removedNode;
        public BaseNode addedNode;
        public BaseNode nodeChanged;

        public Group addedGroups;
        public Group removedGroups;

        public StickyNote addedStickyNotes;
        public StickyNote removedStickyNotes;
    }

    public class BaseGraph
    {
        public string displayName = "";

        //视图位置和尺寸
        public Vector3 position = Vector3.zero;
        public Vector3 scale = Vector3.one;

        //节点
        public List<BaseNode> nodes = new List<BaseNode>();
        //边缘(连接线)
        public List<BaseEdge> edges = new List<BaseEdge>();
        //组
        public List<Group> groups = new List<Group>();
        //注释
        public List<StickyNote> stickyNotes = new List<StickyNote>();
        //视图参数
        public List<ExposedParameter> exposedParameters = new List<ExposedParameter>();
        //所有的固定元素
        public List<PinnedElement> pinnedElements = new List<PinnedElement>();

        //为了访问迅速
        [JsonIgnore]
        public Dictionary<string, BaseNode> nodesPerGUID = new Dictionary<string, BaseNode>();
        [JsonIgnore]
        public Dictionary<string, BaseEdge> edgesPerGUID = new Dictionary<string, BaseEdge>();

        //暴露的参数事件
        public event Action onExposedParameterListChanged;
        public event Action<ExposedParameter> onExposedParameterModified;
        public event Action<ExposedParameter> onExposedParameterValueChanged;

        //视图事件
        [JsonIgnore]
        bool _isEnabled = false;
        [JsonIgnore]
        public bool isEnabled { get => _isEnabled; private set => _isEnabled = value; }
        public event Action onEnabled;
        public event Action<GraphChanges> onGraphChanges;

        [JsonIgnore]
        public Action<BaseGraph, BaseNode> createNodeIdAction;

        public BaseGraph() { }
        public BaseGraph(string displayName)
        {
            this.displayName = displayName;
        }

        #region 初始化

        public void Init(Action<BaseGraph, BaseNode> nodeIdAction)
        {
            createNodeIdAction = nodeIdAction;
            InitData();
            InitGraphElements();
            DestroyBrokenGraphElements();
            isEnabled = true;
            onEnabled?.Invoke();
        }

        void InitData()
        {
            nodesPerGUID = new Dictionary<string, BaseNode>();
            edgesPerGUID = new Dictionary<string, BaseEdge>();
        }

        //初始化视图元素
        void InitGraphElements()
        {
            //节点
            foreach (var node in nodes.ToList())
            {
                nodesPerGUID[node.id] = node;
                node.Init(this);
            }
            //边缘
            foreach (var edge in edges.ToList())
            {
                edge.Init(this);
                edgesPerGUID[edge.id] = edge;
                // 检测
                if (edge.inputPort == null || edge.outputPort == null)
                {
                    DisconnectEdge(edge.id);
                    continue;
                }

                // 添加端口链接
                edge.inputPort.node.OnEdgeConnected(edge);
                edge.outputPort.node.OnEdgeConnected(edge);
            }
        }

        //清理损坏的元素
        void DestroyBrokenGraphElements()
        {
            edges.RemoveAll(e => e.inputNode == null
                || e.outputNode == null
                || string.IsNullOrEmpty(e.outputFieldName)
                || string.IsNullOrEmpty(e.inputFieldName)
            );
            nodes.RemoveAll(n => n == null);
        }

        #endregion

        #region Public

        //添加节点
        public BaseNode AddNode(BaseNode node)
        {
            node.Init(this);
            nodesPerGUID[node.id] = node;
            nodes.Add(node);
            onGraphChanges?.Invoke(new GraphChanges { addedNode = node });
            return node;
        }

        //删除节点
        public void RemoveNode(BaseNode node)
        {
            node.DestroyInternal();

            nodesPerGUID.Remove(node.id);

            nodes.Remove(node);

            onGraphChanges?.Invoke(new GraphChanges { removedNode = node });
        }

        //连接俩个端口
        public BaseEdge ConnectPort(NodePort inputPort, NodePort outputPort, bool autoDisconnectInputs = true)
        {
            var edge = BaseEdge.CreateNewEdge(this, inputPort, outputPort);

            //如果输入端口不支持多连接，我们就删除它们
            if (autoDisconnectInputs && !inputPort.portData.acceptMultipleEdges)
            {
                foreach (var e in inputPort.GetEdges().ToList())
                {
                    DisconnectEdge(e);
                }
            }
            if (autoDisconnectInputs && !outputPort.portData.acceptMultipleEdges)
            {
                foreach (var e in outputPort.GetEdges().ToList())
                {
                    DisconnectEdge(e);
                }
            }

            edges.Add(edge);

            inputPort.node.OnEdgeConnected(edge);
            outputPort.node.OnEdgeConnected(edge);

            onGraphChanges?.Invoke(new GraphChanges { addedEdge = edge });

            return edge;
        }

        //断开两个端口
        public void DisconnectPort(BaseNode inputNode, string inputFieldName, BaseNode outputNode, string outputFieldName)
        {
            edges.RemoveAll(r =>
            {
                bool remove = r.inputNode == inputNode
                && r.outputNode == outputNode
                && r.outputFieldName == outputFieldName
                && r.inputFieldName == inputFieldName;

                if (remove)
                {
                    r.inputNode?.OnEdgeDisconnected(r);
                    r.outputNode?.OnEdgeDisconnected(r);
                    onGraphChanges?.Invoke(new GraphChanges { removedEdge = r });
                }

                return remove;
            });

        }

        //断开边缘
        public void DisconnectEdge(BaseEdge edge) => DisconnectEdge(edge.id);

        //断开边缘
        public void DisconnectEdge(string edgeId)
        {
            List<(BaseNode, BaseEdge)> disconnectEvents = new List<(BaseNode, BaseEdge)>();

            edges.RemoveAll(r =>
            {
                if (r.id == edgeId)
                {
                    disconnectEvents.Add((r.inputNode, r));
                    disconnectEvents.Add((r.outputNode, r));
                    onGraphChanges?.Invoke(new GraphChanges { removedEdge = r });
                }
                return r.id == edgeId;
            });

            foreach (var (node, edge) in disconnectEvents)
                node?.OnEdgeDisconnected(edge);
        }

        //添加分组
        public void AddGroup(Group block)
        {
            groups.Add(block);
            onGraphChanges?.Invoke(new GraphChanges { addedGroups = block });
        }

        //删除分组
        public void RemoveGroup(Group block)
        {
            groups.Remove(block);
            onGraphChanges?.Invoke(new GraphChanges { removedGroups = block });
        }

        //打开固定元素
        public PinnedElement OpenPinned(Type viewType)
        {
            var pinned = pinnedElements.Find(p => p.editorTypeFullName == viewType.FullName);

            if (pinned == null)
            {
                pinned = new PinnedElement(viewType);
                pinnedElements.Add(pinned);
            }
            else
                pinned.opened = true;

            return pinned;
        }

        //关闭固定元素
        public void ClosePinned(Type viewType)
        {
            var pinned = pinnedElements.Find(p => p.editorTypeFullName == viewType.FullName);

            pinned.opened = false;
        }

        //添加注释
        public void AddStickyNote(StickyNote note)
        {
            stickyNotes.Add(note);
            onGraphChanges?.Invoke(new GraphChanges { addedStickyNotes = note });
        }

        //删除注释
        public void RemoveStickyNote(StickyNote note)
        {
            stickyNotes.Remove(note);
            onGraphChanges?.Invoke(new GraphChanges { removedStickyNotes = note });
        }

        #endregion

        #region 视图参数

        public ExposedParameter GetExposedParameter(string name)
        {
            return exposedParameters.FirstOrDefault(e => e.name == name);
        }

        public ExposedParameter GetExposedParameterFromId(string id)
        {
            return exposedParameters.FirstOrDefault(e => e?.id == id);
        }

        public object GetParameterValue(string name) => exposedParameters.FirstOrDefault(p => p.name == name)?.value;

        public T GetParameterValue<T>(string name) => (T)GetParameterValue(name);

        public bool SetParameterValue(string name, object value)
        {
            var e = exposedParameters.FirstOrDefault(p => p.name == name);

            if (e == null)
                return false;

            e.value = value;

            return true;
        }

        public string AddExposedParameter(string name, Type type, object value = null)
        {
            if (!type.IsSubclassOf(typeof(ExposedParameter)))
            {
                Debug.LogError($"该类型没有继承ExposedParameter {type}");
            }

            var param = Activator.CreateInstance(type) as ExposedParameter;
            if (param.GetValueType().IsValueType)
                value = Activator.CreateInstance(param.GetValueType());

            param.Init(name, value);
            exposedParameters.Add(param);

            onExposedParameterListChanged?.Invoke();

            return param.id;
        }

        public string AddExposedParameter(ExposedParameter parameter)
        {
            string id = Guid.NewGuid().ToString();

            parameter.id = id;
            exposedParameters.Add(parameter);

            onExposedParameterListChanged?.Invoke();

            return id;
        }

        public void RemoveExposedParameter(ExposedParameter ep)
        {
            exposedParameters.Remove(ep);

            onExposedParameterListChanged?.Invoke();
        }

        public void RemoveExposedParameter(string id)
        {
            if (exposedParameters.RemoveAll(e => e.id == id) != 0)
                onExposedParameterListChanged?.Invoke();
        }

        public void UpdateExposedParameter(string id, object value)
        {
            var param = exposedParameters.Find(e => e.id == id);
            if (param == null)
                return;

            if (value != null && !param.GetValueType().IsAssignableFrom(value.GetType()))
                throw new Exception("参数转换失败 " + param.name + ": from " + param.GetValueType() + " to " + value.GetType().AssemblyQualifiedName);

            param.value = value;
            onExposedParameterModified?.Invoke(param);
        }

        public void UpdateExposedParameterName(ExposedParameter parameter, string name)
        {
            parameter.name = name;
            onExposedParameterModified?.Invoke(parameter);
        }

        //通知参数变换
        public void NotifyExposedParameterChanged(ExposedParameter parameter)
        {
            onExposedParameterModified?.Invoke(parameter);
        }

        public void NotifyExposedParameterValueChanged(ExposedParameter parameter)
        {
            onExposedParameterValueChanged?.Invoke(parameter);
        }

        #endregion

        #region 事件处理

        //通知节点改变————>向下传递消息
        public void NotifyNodeChanged(BaseNode node) => onGraphChanges?.Invoke(new GraphChanges { nodeChanged = node });

        #endregion

        #region 静态方法

        //判断俩个类型是否可以相连
        public static bool TypesAreConnectable(Type t1, Type t2)
        {
            if (t1 == null || t2 == null)
                return false;

            if (t2.IsReallyAssignableFrom(t1))
                return true;

            return false;
        }

        #endregion
    }
}
