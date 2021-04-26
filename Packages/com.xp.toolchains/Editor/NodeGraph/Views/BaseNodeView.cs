using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using NodeView = UnityEditor.Experimental.GraphView.Node;

namespace XPToolchains.NodeGraph
{
    //节点视图
    [NodeCustomEditor(typeof(BaseNode))]
    public class BaseNodeView : NodeView
    {
        readonly string baseNodeStyle = "BaseNodeView";

        public BaseNode nodeTarget;

        public BaseGraphView graphView { private set; get; }
        public List<PortView> inputPortViews = new List<PortView>();
        public List<PortView> outputPortViews = new List<PortView>();
        //字段和端口视图映射
        protected Dictionary<string, List<PortView>> portsPerFieldName = new Dictionary<string, List<PortView>>();

        //布局元素
        private VisualElement inputContainerElement;
        protected VisualElement rightTitleContainer;
        protected VisualElement topPortContainer;
        protected VisualElement bottomPortContainer;
        public VisualElement controlsContainer;

        //事件
        public event Action<PortView> onPortConnected;
        public event Action<PortView> onPortDisconnected;

        Dictionary<string, List<(object value, VisualElement target)>> visibleConditions = new Dictionary<string, List<(object value, VisualElement target)>>();
        Dictionary<string, VisualElement> hideElementIfConnected = new Dictionary<string, VisualElement>();
        Dictionary<FieldInfo, List<VisualElement>> fieldControlsMap = new Dictionary<FieldInfo, List<VisualElement>>();

        #region 初始化

        public void Init(BaseGraphView owner, BaseNode node)
        {
            //样式表
            styleSheets.Add(NodeGraphDefine.LoadUSS(baseNodeStyle));
            if (!string.IsNullOrEmpty(node.layoutStyle))
                styleSheets.Add(NodeGraphDefine.LoadUSS(node.layoutStyle));

            nodeTarget = node;
            graphView = owner;

            //设置不可删除特性
            if (!node.deletable)
                capabilities = capabilities & ~Capabilities.Deletable;

            node.onPortsUpdated += UpdatePortsForField;

            //初始化界面
            InitView();
            InitPorts();

            //刷新展开
            RefreshExpandedState();

            RefreshPorts();

            //注册图形变换事件
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            OnGeometryChanged(null);

            if (GetType().GetMethod(nameof(Enable), new Type[] { }).DeclaringType != typeof(BaseNodeView))
                ExceptionToLog.Call(() => Enable());
            else
                ExceptionToLog.Call(() => Enable(false));
        }

        //初始化显示
        void InitView()
        {
            //节点名
            RefreshTitle();

            //布局区域
            CreateContainers();

            SetPosition(nodeTarget.position);
            SetNodeColor(nodeTarget.color);

            Undo.undoRedoPerformed += UpdateFieldValues;
        }

        //初始化端口
        void InitPorts()
        {
            var listener = graphView.connectorListener;

            foreach (var inputPort in nodeTarget.inputPorts)
            {
                AddPortView(inputPort.fieldInfo, Direction.Input, listener, inputPort.portData);
            }

            foreach (var outputPort in nodeTarget.outputPorts)
            {
                AddPortView(outputPort.fieldInfo, Direction.Output, listener, outputPort.portData);
            }
        }

        #endregion

        #region 创建布局区域

        protected void CreateContainers()
        {
            controlsContainer = new VisualElement { name = "controls" };
            controlsContainer.AddToClassList("NodeControls");
            mainContainer.Add(controlsContainer);

            rightTitleContainer = new VisualElement { name = "RightTitleContainer" };
            titleContainer.Add(rightTitleContainer);

            topPortContainer = new VisualElement { name = "TopPortContainer" };
            this.Insert(0, topPortContainer);

            bottomPortContainer = new VisualElement { name = "BottomPortContainer" };
            this.Add(bottomPortContainer);

            inputContainerElement = new VisualElement { name = "input-container" };
            mainContainer.parent.Add(inputContainerElement);
            inputContainerElement.SendToBack();
            inputContainerElement.pickingMode = PickingMode.Ignore;
        }

        #endregion

        #region 重写父类

        //设置位置
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            graphView.RegisterCompleteObjectUndo("Moved graph node");
            nodeTarget.position = newPos;
        }

        //展开
        public override bool expanded
        {
            get { return base.expanded; }
            set
            {
                base.expanded = value;
                nodeTarget.expanded = value;
            }
        }

        #endregion

        #region 子类重写函数

        public virtual void Enable(bool fromInspector = false) => DrawDefaultInspector(fromInspector);
        public virtual void Enable() => DrawDefaultInspector(false);
        public virtual void OnRemoved() { }
        public virtual void OnCreated() { }

        #endregion

        #region 绘制

        private void RefreshTitle()
        {
            //节点名
            string idTip = "";
            int tempId;
            if (int.TryParse(nodeTarget.id, out tempId))
            {
                idTip = string.Format("({0})", tempId);
            }
            title = string.IsNullOrEmpty(nodeTarget.name) ? nodeTarget.GetType().Name + idTip : nodeTarget.name + idTip;
        }

        //绘制Inspector面板
        protected virtual void DrawDefaultInspector(bool fromInspector = false)
        {
            var fields = nodeTarget.GetType()
                        .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(f => f.DeclaringType != typeof(BaseNode));

            //排序
            //fields = nodeTarget.OverrideFieldOrder(fields).Reverse();

            foreach (var field in fields)
            {

                //不是可序列化的
                if ((!field.IsPublic && field.GetCustomAttribute(typeof(SerializeField)) == null) || field.IsNotSerialized)
                {
                    AddEmptyField(field, fromInspector);
                    continue;
                }

                //输入输出字段但不是可序列化的
                bool hasInputAttribute = field.GetCustomAttribute(typeof(InputAttribute)) != null;
                bool hasInputOrOutputAttribute = hasInputAttribute || field.GetCustomAttribute(typeof(OutputAttribute)) != null;
                bool showAsDrawer = !fromInspector && field.GetCustomAttribute(typeof(ShowAsDrawer)) != null;
                if (field.GetCustomAttribute(typeof(SerializeField)) == null && hasInputOrOutputAttribute && !showAsDrawer)
                {
                    AddEmptyField(field, fromInspector);
                    continue;
                }

                //非序列化以及在Inspector隐藏
                if (field.GetCustomAttribute(typeof(System.NonSerializedAttribute)) != null || field.GetCustomAttribute(typeof(HideInInspector)) != null)
                {
                    AddEmptyField(field, fromInspector);
                    continue;
                }

                var showInputDrawer = field.GetCustomAttribute(typeof(InputAttribute)) != null && field.GetCustomAttribute(typeof(SerializeField)) != null;
                showInputDrawer |= field.GetCustomAttribute(typeof(InputAttribute)) != null && field.GetCustomAttribute(typeof(ShowAsDrawer)) != null;
                showInputDrawer &= !fromInspector;
                showInputDrawer &= !typeof(IList).IsAssignableFrom(field.FieldType);

                var elem = AddControlField(field, ObjectNames.NicifyVariableName(field.Name), showInputDrawer);
                if (hasInputAttribute)
                {
                    hideElementIfConnected[field.Name] = elem;

                    // 有一个连接，立即隐藏字段
                    if (portsPerFieldName.TryGetValue(field.Name, out var pvs))
                        if (pvs.Any(pv => pv.GetEdges().Count > 0))
                            elem.style.display = DisplayStyle.None;
                }
            }
        }

        private void AddEmptyField(FieldInfo field, bool fromInspector)
        {
            if (field.GetCustomAttribute(typeof(InputAttribute)) == null || fromInspector)
                return;

            if (field.GetCustomAttribute<VerticalAttribute>() != null)
                return;

            var box = new VisualElement { name = name };
            box.AddToClassList("port-input-element");
            box.AddToClassList("empty");
            inputContainerElement.Add(box);
        }

        protected VisualElement AddControlField(FieldInfo field, string label = null, bool showInputDrawer = false, Action valueChangedCallback = null)
        {
            if (field == null)
                return null;

            VisualElement element = FieldFactory.CreateField(field.FieldType, field.GetValue(nodeTarget), (newValue) =>
            {
                //注册注销标记
                graphView.RegisterCompleteObjectUndo("Updated " + newValue);
                //设置值
                field.SetValue(nodeTarget, newValue);
                //通知节点改变
                NotifyNodeChanged();
                valueChangedCallback?.Invoke();
                //更新字段可见性
                UpdateFieldVisibility(field.Name, newValue);
                //更新一个字段代表很多数据的值
                UpdateOtherFieldValue(field, newValue);
            }, showInputDrawer ? "" : label);

            if (!fieldControlsMap.TryGetValue(field, out var inputFieldList))
                inputFieldList = fieldControlsMap[field] = new List<VisualElement>();
            inputFieldList.Add(element);

            if (element != null)
            {
                //抽屉
                if (showInputDrawer)
                {
                    var box = new VisualElement { name = field.Name };
                    box.AddToClassList("port-input-element");
                    box.Add(element);
                    inputContainerElement.Add(box);
                }
                else
                {
                    controlsContainer.Add(element);
                }
            }
            else
            {
                // 空字段
                if (showInputDrawer) AddEmptyField(field, false);
            }

            return element;
        }

        //更新字段值
        static MethodInfo specificUpdateOtherFieldValue = typeof(BaseNodeView).GetMethod(nameof(UpdateOtherFieldValueSpecific), BindingFlags.NonPublic | BindingFlags.Instance);
        void UpdateOtherFieldValue(FieldInfo info, object newValue)
        {
            var fieldType = info.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) ? typeof(UnityEngine.Object) : info.FieldType;
            var genericUpdate = specificUpdateOtherFieldValue.MakeGenericMethod(fieldType);
            genericUpdate.Invoke(this, new object[] { info, newValue });
        }

        void UpdateOtherFieldValueSpecific<T>(FieldInfo field, object newValue)
        {
            foreach (var inputField in fieldControlsMap[field])
            {
                var notify = inputField as INotifyValueChanged<T>;
                if (notify != null)
                    notify.SetValueWithoutNotify((T)newValue);
            }
        }

        //字段可见性
        void UpdateFieldVisibility(string fieldName, object newValue)
        {
            if (visibleConditions.TryGetValue(fieldName, out var list))
            {
                foreach (var elem in list)
                {
                    if (newValue.Equals(elem.value))
                        elem.target.style.display = DisplayStyle.Flex;
                    else
                        elem.target.style.display = DisplayStyle.None;
                }
            }
        }

        #endregion

        #region 事件处理

        void UpdatePortsForField(string fieldName)
        {
            RefreshPorts();
        }

        //图形变换调用
        void OnGeometryChanged(GeometryChangedEvent evt)
        {
        }

        public void NotifyNodeChanged()
        {
            RefreshTitle();
            graphView.graph.NotifyNodeChanged(nodeTarget);
        }

        #endregion

        #region Set

        protected virtual void SetNodeColor(Color color)
        {
            titleContainer.style.borderBottomColor = new StyleColor(color);
            titleContainer.style.borderBottomWidth = new StyleFloat(color.a > 0 ? 5f : 0f);
        }

        #endregion

        #region Add

        protected virtual PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener listener)
            => PortView.CreatePortView(direction, fieldInfo, portData, listener);

        public PortView AddPortView(FieldInfo fieldInfo, Direction direction, BaseEdgeConnectorListener listener, PortData portData)
        {
            PortView p = CreatePortView(direction, fieldInfo, portData, listener);

            if (p.direction == Direction.Input)
            {
                inputPortViews.Add(p);

                if (portData.vertical)
                    topPortContainer.Add(p);
                else
                    inputContainer.Add(p);
            }
            else
            {
                outputPortViews.Add(p);

                if (portData.vertical)
                    bottomPortContainer.Add(p);
                else
                    outputContainer.Add(p);
            }

            p.Init(this, portData?.displayName);

            List<PortView> ports;
            portsPerFieldName.TryGetValue(p.fieldName, out ports);
            if (ports == null)
            {
                ports = new List<PortView>();
                portsPerFieldName[p.fieldName] = ports;
            }
            ports.Add(p);

            return p;
        }

        public void RemovePort(PortView p)
        {
            var edgesCopy = p.GetEdges().ToList();
            foreach (var e in edgesCopy)
                graphView.Disconnect(e, refreshPorts: false);

            if (p.direction == Direction.Input)
            {
                if (inputPortViews.Remove(p))
                    p.RemoveFromHierarchy();
            }
            else
            {
                if (outputPortViews.Remove(p))
                    p.RemoveFromHierarchy();
            }

            List<PortView> ports;
            portsPerFieldName.TryGetValue(p.fieldName, out ports);
            ports.Remove(p);
        }

        #endregion

        #region Get

        static MethodInfo specificGetValue = typeof(BaseNodeView).GetMethod(nameof(GetInputFieldValueSpecific), BindingFlags.NonPublic | BindingFlags.Instance);
        object GetInputFieldValue(FieldInfo info)
        {
            // Warning: Keep in sync with FieldFactory CreateField
            var fieldType = info.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) ? typeof(UnityEngine.Object) : info.FieldType;
            var genericUpdate = specificGetValue.MakeGenericMethod(fieldType);

            return genericUpdate.Invoke(this, new object[] { info });
        }

        object GetInputFieldValueSpecific<T>(FieldInfo field)
        {
            if (fieldControlsMap.TryGetValue(field, out var list))
            {
                foreach (var inputField in list)
                {
                    if (inputField is INotifyValueChanged<T> notify)
                        return notify.value;
                }
            }
            return null;
        }

        public List<PortView> GetPortViewsFromFieldName(string fieldName)
        {
            List<PortView> ret;

            portsPerFieldName.TryGetValue(fieldName, out ret);

            return ret;
        }

        public PortView GetPortViewFromFieldName(string fieldName, string identifier)
        {
            List<PortView> resPorts = GetPortViewsFromFieldName(fieldName);
            return resPorts?.FirstOrDefault(pv =>
            {
                return (pv.portData.id == identifier) || (String.IsNullOrEmpty(pv.portData.id) && String.IsNullOrEmpty(identifier));
            });
        }
        public static Rect GetNodeRect(Node node, float left = int.MaxValue, float top = int.MaxValue)
        {
            return new Rect(
                new Vector2(left != int.MaxValue ? left : node.style.left.value.value, top != int.MaxValue ? top : node.style.top.value.value),
                new Vector2(node.style.width.value.value, node.style.height.value.value)
            );
        }

        public PortView GetFirstPortViewFromFieldName(string fieldName)
        {
            return GetPortViewsFromFieldName(fieldName)?.First();
        }

        #endregion

        #region Connected

        internal void OnPortConnected(PortView port)
        {
            if (port.direction == Direction.Input && inputContainerElement?.Q(port.fieldName) != null)
                inputContainerElement.Q(port.fieldName).AddToClassList("empty");

            if (hideElementIfConnected.TryGetValue(port.fieldName, out var elem))
                elem.style.display = DisplayStyle.None;

            onPortConnected?.Invoke(port);
        }

        #endregion

        #region Disconnected

        internal void OnPortDisconnected(PortView port)
        {
            if (port.direction == Direction.Input && inputContainerElement?.Q(port.fieldName) != null)
            {
                inputContainerElement.Q(port.fieldName).RemoveFromClassList("empty");

                if (nodeTarget.nodeFields.TryGetValue(port.fieldName, out var fieldInfo))
                {
                    var valueBeforeConnection = GetInputFieldValue(fieldInfo.info);

                    if (valueBeforeConnection != null)
                    {
                        fieldInfo.info.SetValue(nodeTarget, valueBeforeConnection);
                    }
                }
            }

            if (hideElementIfConnected.TryGetValue(port.fieldName, out var elem))
                elem.style.display = DisplayStyle.Flex;

            onPortDisconnected?.Invoke(port);
        }

        #endregion

        #region 更新

        public void ForceUpdatePorts()
        {
            nodeTarget.UpdateAllPorts();

            RefreshPorts();
        }

        void UpdateFieldValues()
        {
            foreach (var kp in fieldControlsMap)
                UpdateOtherFieldValue(kp.Key, kp.Key.GetValue(nodeTarget));
        }

        #endregion
    }
}
