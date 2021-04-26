using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

namespace XPToolchains.NodeGraph
{
    //节点视图
    public class BaseGraphView : GraphView, IDisposable
    {
        //数据
        public BaseGraph graph;

        //边缘的连接侦听
        public BaseEdgeConnectorListener connectorListener;

        //节点视图
        public List<BaseNodeView> nodeViews = new List<BaseNodeView>();
        //为了访问迅速
        public Dictionary<BaseNode, BaseNodeView> nodeViewsPerNode = new Dictionary<BaseNode, BaseNodeView>();

        //边缘视图
        public List<EdgeView> edgeViews = new List<EdgeView>();
        //组视图
        public List<GroupView> groupViews = new List<GroupView>();
        //固定元素视图
        Dictionary<Type, PinnedElementView> pinnedElements = new Dictionary<Type, PinnedElementView>();

#if UNITY_2020_1_OR_NEWER
        //注释视图
        public List<StickyNoteView> stickyNoteViews = new List<StickyNoteView>();
#endif

        //创建节点视图
        CreateNodeMenuWindow createNodeMenu;

        //视图初始化事件
        public event Action initialized;
        //参数列表改变事件
        public event Action onExposedParameterListChanged;
        //参数改变事件
        public event Action<ExposedParameter> onExposedParameterModified;

        public ExposedParameterFieldFactory exposedParameterFactory { get; private set; }

        public event Action onGraphUpdate;

        public BaseGraphWindow baseGraphWindow;

        public BaseGraphView(BaseGraphWindow window)
        {
            baseGraphWindow = window;

            Debug.LogError($"{baseGraphWindow}");

            RegInternalCallBack();
            RegInputEvent();

            //初始化操作器
            InitManipulators();

            //设置缩放
            SetupZoom(0.05f, 2f);

            //添加创建节点菜单
            createNodeMenu = ScriptableObject.CreateInstance<CreateNodeMenuWindow>();
            createNodeMenu.Initialize(this, window);

            //拉伸到父级大小
            this.StretchToParentSize();
        }

        //注册内部回调
        private void RegInternalCallBack()
        {
            serializeGraphElements = SerializeGraphElementsCallback;
            canPasteSerializedData = CanPasteSerializedDataCallback;
            unserializeAndPaste = UnserializeAndPasteCallback;
            graphViewChanged = GraphViewChangedCallback;
            viewTransformChanged = ViewTransformChangedCallback;
            elementResized = ElementResizedCallback;
        }

        //注册输入事件
        private void RegInputEvent()
        {
            RegisterCallback<DragUpdatedEvent>(DragUpdatedCallback);
        }

        #region 初始化

        public void Init(BaseGraph graph)
        {
            if (this.graph!=null)
            {
                baseGraphWindow.SaveGraph();
            }
            //清理数据
            ClearGraphElements();

            this.graph = graph;

            exposedParameterFactory = new ExposedParameterFieldFactory(graph);

            UpdateSerializedProperties();

            connectorListener = CreateEdgeConnectorListener();

            InitGraphView();
            InitNodeViews();
            InitEdgeViews();
            InitPinnedViews();
            InitGroups();
            InitStickyNotes();

            initialized?.Invoke();

            InitializeView();
        }

        //初始化视图面板
        void InitGraphView()
        {
            graph.onExposedParameterListChanged += OnExposedParameterListChanged;
            graph.onExposedParameterModified += (s) => onExposedParameterModified?.Invoke(s);
            graph.onGraphChanges += GraphChangesCallback;
            viewTransform.position = graph.position;
            viewTransform.scale = graph.scale;

            //创建节点请求（打开节点菜单）
            nodeCreationRequest = (c) => SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), createNodeMenu);
        }

        void InitNodeViews()
        {
            graph.nodes.RemoveAll(n => n == null);

            foreach (var node in graph.nodes)
            {
                var v = AddNodeView(node);
            }
        }

        void InitEdgeViews()
        {
            graph.edges.RemoveAll(edge => edge == null || edge.inputNode == null || edge.outputNode == null);

            foreach (var serializedEdge in graph.edges)
            {
                nodeViewsPerNode.TryGetValue(serializedEdge.inputNode, out var inputNodeView);
                nodeViewsPerNode.TryGetValue(serializedEdge.outputNode, out var outputNodeView);
                if (inputNodeView == null || outputNodeView == null)
                    continue;

                var edgeView = CreateEdgeView();
                edgeView.userData = serializedEdge;
                edgeView.input = inputNodeView.GetPortViewFromFieldName(serializedEdge.inputFieldName, serializedEdge.inputPortId);
                edgeView.output = outputNodeView.GetPortViewFromFieldName(serializedEdge.outputFieldName, serializedEdge.outputPortId);

                ConnectView(edgeView);
            }
        }

        void InitGroups()
        {
            foreach (var group in graph.groups)
                AddGroupView(group);
        }

        void InitStickyNotes()
        {
#if UNITY_2020_1_OR_NEWER
            foreach (var group in graph.stickyNotes)
                AddStickyNoteView(group);
#endif
        }

        //初始化固定视图
        void InitPinnedViews()
        {
            foreach (var pinnedElement in graph.pinnedElements)
            {
                if (pinnedElement.opened)
                    OpenPinned(Type.GetType(pinnedElement.editorTypeFullName));
            }
        }

        #endregion

        #region 按键事件

        void DragUpdatedCallback(DragUpdatedEvent e)
        {
            var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            bool dragging = false;

            if (dragData != null)
            {
                // Handle drag from exposed parameter view
                if (dragData.OfType<ExposedParameterFieldView>().Any())
                {
                    dragging = true;
                }
            }

            if (dragging)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }
        }

        #endregion

        #region 内部回调

        string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            var data = new CopyPasteHelper();

            foreach (BaseNodeView nodeView in elements.Where(e => e is BaseNodeView))
            {
                data.copiedNodes.Add(JsonSerializer.SerializeNode(nodeView.nodeTarget));
                foreach (var port in nodeView.nodeTarget.GetAllPorts())
                {
                    if (port.portData.vertical)
                    {
                        foreach (var edge in port.GetEdges())
                            data.copiedEdges.Add(JsonSerializer.Serialize(edge));
                    }
                }
            }

            foreach (GroupView groupView in elements.Where(e => e is GroupView))
                data.copiedGroups.Add(JsonSerializer.Serialize(groupView.group));

            foreach (EdgeView edgeView in elements.Where(e => e is EdgeView))
                data.copiedEdges.Add(JsonSerializer.Serialize(edgeView.Edge));

            ClearSelection();

            return JsonUtility.ToJson(data, true);
        }

        bool CanPasteSerializedDataCallback(string serializedData)
        {
            try
            {
                return JsonUtility.FromJson(serializedData, typeof(CopyPasteHelper)) != null;
            }
            catch
            {
                return false;
            }
        }

        //反序列化和粘贴回调
        void UnserializeAndPasteCallback(string operationName, string serializedData)
        {
            var data = JsonUtility.FromJson<CopyPasteHelper>(serializedData);

            RegisterCompleteObjectUndo(operationName);

            Dictionary<string, BaseNode> copiedNodesMap = new Dictionary<string, BaseNode>();

            foreach (var serializedNode in data.copiedNodes)
            {
                var node = JsonSerializer.DeserializeNode(serializedNode);

                if (node == null)
                    continue;

                string sourceGUID = node.id;
                graph.nodesPerGUID.TryGetValue(sourceGUID, out var sourceNode);
                node.graph = graph;
                //Call OnNodeCreated on the new fresh copied node
                node.CreatedNodeId();
                //And move a bit the new node
                node.position.position += new Vector2(20, 20);

                var newNodeView = AddNode(node);
                copiedNodesMap[sourceGUID] = node;

                //Select the new node
                AddToSelection(nodeViewsPerNode[node]);
            }

            foreach (var serializedGroup in data.copiedGroups)
            {
                var group = JsonSerializer.Deserialize<Group>(serializedGroup);

                //Same than for node
                group.OnCreated();

                // try to centre the created node in the screen
                group.position.position += new Vector2(20, 20);

                var oldGUIDList = group.innerNodeIds.ToList();
                group.innerNodeIds.Clear();
                foreach (var guid in oldGUIDList)
                {
                    graph.nodesPerGUID.TryGetValue(guid, out var node);

                    // In case group was copied from another graph
                    if (node == null)
                    {
                        copiedNodesMap.TryGetValue(guid, out node);
                        group.innerNodeIds.Add(node.id);
                    }
                    else
                    {
                        group.innerNodeIds.Add(copiedNodesMap[guid].id);
                    }
                }

                AddGroup(group);
            }

            foreach (var serializedEdge in data.copiedEdges)
            {
                var edge = JsonSerializer.Deserialize<BaseEdge>(serializedEdge);
                edge.InitCopy(graph);
                edge.Init(graph);


                // Find port of new nodes:
                copiedNodesMap.TryGetValue(edge.inputNode.id, out var oldInputNode);
                copiedNodesMap.TryGetValue(edge.outputNode.id, out var oldOutputNode);

                // We avoid to break the graph by replacing unique connections:
                if (oldInputNode == null && !edge.inputPort.portData.acceptMultipleEdges || !edge.outputPort.portData.acceptMultipleEdges)
                    continue;

                oldInputNode = oldInputNode ?? edge.inputNode;
                oldOutputNode = oldOutputNode ?? edge.outputNode;

                var inputPort = oldInputNode.GetPort(edge.inputPort.fieldName, edge.inputPortId);
                var outputPort = oldOutputNode.GetPort(edge.outputPort.fieldName, edge.outputPortId);

                var newEdge = BaseEdge.CreateNewEdge(graph, inputPort, outputPort);

                if (nodeViewsPerNode.ContainsKey(oldInputNode) && nodeViewsPerNode.ContainsKey(oldOutputNode))
                {
                    var edgeView = CreateEdgeView();
                    edgeView.userData = newEdge;
                    edgeView.input = nodeViewsPerNode[oldInputNode].GetPortViewFromFieldName(newEdge.inputFieldName, newEdge.inputPortId);
                    edgeView.output = nodeViewsPerNode[oldOutputNode].GetPortViewFromFieldName(newEdge.outputFieldName, newEdge.outputPortId);

                    Connect(edgeView);
                }
            }
        }

        GraphViewChange GraphViewChangedCallback(GraphViewChange changes)
        {
            if (changes.elementsToRemove != null)
            {
                RegisterCompleteObjectUndo("Remove Graph Elements");

                // Destroy priority of objects
                // We need nodes to be destroyed first because we can have a destroy operation that uses node connections
                changes.elementsToRemove.Sort((e1, e2) =>
                {
                    int GetPriority(GraphElement e)
                    {
                        if (e is BaseNodeView)
                            return 0;
                        else
                            return 1;
                    }
                    return GetPriority(e1).CompareTo(GetPriority(e2));
                });

                //Handle ourselves the edge and node remove
                changes.elementsToRemove.RemoveAll(e =>
                {

                    switch (e)
                    {
                        case EdgeView edge:
                            Disconnect(edge);
                            return true;
                        case BaseNodeView nodeView:
                            // For vertical nodes, we need to delete them ourselves as it's not handled by GraphView
                            foreach (var pv in nodeView.inputPortViews.Concat(nodeView.outputPortViews))
                                if (pv.orientation == Orientation.Vertical)
                                    foreach (var edge in pv.GetEdges().ToList())
                                        Disconnect(edge);

                            ExceptionToLog.Call(() => nodeView.OnRemoved());
                            graph.RemoveNode(nodeView.nodeTarget);
                            UpdateSerializedProperties();
                            RemoveElement(nodeView);
                            return true;
                        case GroupView group:
                            graph.RemoveGroup(group.group);
                            UpdateSerializedProperties();
                            RemoveElement(group);
                            return true;
                        case ExposedParameterFieldView blackboardField:
                            graph.RemoveExposedParameter(blackboardField.parameter);
                            UpdateSerializedProperties();
                            return true;
#if UNITY_2020_1_OR_NEWER
                        case StickyNoteView stickyNoteView:
                            graph.RemoveStickyNote(stickyNoteView.note);
                            UpdateSerializedProperties();
                            RemoveElement(stickyNoteView);
                            return true;
#endif
                    }

                    return false;
                });
            }

            return changes;
        }

        void GraphChangesCallback(GraphChanges changes)
        {
            if (changes.removedEdge != null)
            {
                var edge = edgeViews.FirstOrDefault(e => e.Edge == changes.removedEdge);

                DisconnectView(edge);
            }
        }

        void ViewTransformChangedCallback(GraphView view)
        {
            if (graph != null)
            {
                graph.position = viewTransform.position;
                graph.scale = viewTransform.scale;
            }
        }

        void ElementResizedCallback(VisualElement elem)
        {
            var groupView = elem as GroupView;

            if (groupView != null)
                groupView.group.size = groupView.GetPosition().size;
        }

        #endregion

        #region 重写内部实现

        //获得端口
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            compatiblePorts.AddRange(ports.ToList().Where(p =>
            {
                var portView = p as PortView;

                if (portView.nodeView == (startPort as PortView).nodeView)
                    return false;

                if (p.direction == startPort.direction)
                    return false;

                //Check for type assignability
                if (!BaseGraph.TypesAreConnectable(startPort.portType, p.portType))
                    return false;

                //Check if the edge already exists
                if (portView.GetEdges().Any(e => e.input == startPort || e.output == startPort))
                    return false;

                return true;
            }));

            return compatiblePorts;
        }

        #endregion

        #region 子类重写

        protected virtual BaseEdgeConnectorListener CreateEdgeConnectorListener()
         => new BaseEdgeConnectorListener(this);

        //过滤节点
        public virtual IEnumerable<(string path, Type type)> FilterCreateNodeMenuEntries()
        {
            foreach ((string path, Type type) nodeMenuItem in NodeProvider.GetNodeMenuEntries(graph))
            {
                if (!NodeProvider.FilterNodeByNameSpace(nodeMenuItem.type, baseGraphWindow))
                    continue;

                yield return nodeMenuItem;
            }
        }

        //初始化操作器
        protected virtual void InitManipulators()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        protected virtual void Reload() { }

        protected virtual void InitializeView() { }

        #endregion

        #region 更新

        void UpdateSerializedProperties()
        {
            onGraphUpdate?.Invoke();
        }

        #endregion

        #region 右键菜单

        //创建菜单
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            BuildGroupContextualMenu(evt);
            BuildStickyNoteContextualMenu(evt);
            base.BuildContextualMenu(evt);
        }

        protected virtual void BuildGroupContextualMenu(ContextualMenuPopulateEvent evt)
        {
            Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            evt.menu.AppendAction("New Group", (e) => AddSelectionsToGroup(AddGroup(new Group("New Group", position))), DropdownMenuAction.AlwaysEnabled);
        }

        //添加注释
        protected virtual void BuildStickyNoteContextualMenu(ContextualMenuPopulateEvent evt)
        {
#if UNITY_2020_1_OR_NEWER
            Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            evt.menu.AppendAction("注释", (e) => AddStickyNote(new StickyNote("New Note", position)), DropdownMenuAction.AlwaysEnabled);
#endif
        }

        #endregion

        #region 节点 Node

        public BaseNodeView AddNode(BaseNode node)
        {
            graph.AddNode(node);
            UpdateSerializedProperties();
            var view = AddNodeView(node);
            ExceptionToLog.Call(() => view.OnCreated());
            return view;
        }

        public BaseNodeView AddNodeView(BaseNode node)
        {
            var viewType = NodeProvider.GetNodeViewTypeFromType(node.GetType());

            if (viewType == null)
                viewType = typeof(BaseNodeView);
            var baseNodeView = Activator.CreateInstance(viewType) as BaseNodeView;
            baseNodeView.Init(this, node);
            AddElement(baseNodeView);

            nodeViews.Add(baseNodeView);
            nodeViewsPerNode[node] = baseNodeView;

            return baseNodeView;
        }

        void RemoveNodeViews()
        {
            foreach (var nodeView in nodeViews)
                RemoveElement(nodeView);
            nodeViews.Clear();
            nodeViewsPerNode.Clear();
        }

        #endregion

        #region 节点组 Group

        public GroupView AddGroup(Group block)
        {
            graph.AddGroup(block);
            block.OnCreated();
            return AddGroupView(block);
        }

        public GroupView AddGroupView(Group block)
        {
            var c = new GroupView();

            c.Initialize(this, block);

            AddElement(c);

            groupViews.Add(c);
            return c;
        }

        public void RemoveGroups()
        {
            foreach (var groupView in groupViews)
                RemoveElement(groupView);
            groupViews.Clear();
        }

        public void AddSelectionsToGroup(GroupView view)
        {
            foreach (var selectedNode in selection)
            {
                if (selectedNode is BaseNodeView)
                {
                    if (groupViews.Exists(x => x.ContainsElement(selectedNode as BaseNodeView)))
                        continue;

                    view.AddElement(selectedNode as BaseNodeView);
                }
            }
        }

        #endregion

        #region 边缘 Edge

        public virtual EdgeView CreateEdgeView()
        {
            return new EdgeView();
        }

        public void RemoveEdges()
        {
            foreach (var edge in edgeViews)
                RemoveElement(edge);
            edgeViews.Clear();
        }

        #endregion

        #region 固定视图 PinnedElement

        public void ToggleView<T>() where T : PinnedElementView
        {
            ToggleView(typeof(T));
        }

        public void ToggleView(Type type)
        {
            PinnedElementView view;
            pinnedElements.TryGetValue(type, out view);

            if (view == null)
                OpenPinned(type);
            else
                ClosePinned(type, view);
        }

        public void OpenPinned<T>() where T : PinnedElementView
        {
            OpenPinned(typeof(T));
        }

        public void OpenPinned(Type type)
        {
            PinnedElementView view;

            if (type == null)
                return;

            PinnedElement elem = graph.OpenPinned(type);

            if (!pinnedElements.ContainsKey(type))
            {
                view = Activator.CreateInstance(type) as PinnedElementView;
                if (view == null)
                    return;
                pinnedElements[type] = view;
                view.InitializeGraphView(elem, this);
            }
            view = pinnedElements[type];

            if (!Contains(view))
                Add(view);
        }

        public void ClosePinned<T>(PinnedElementView view) where T : PinnedElementView
        {
            ClosePinned(typeof(T), view);
        }

        public void ClosePinned(Type type, PinnedElementView elem)
        {
            pinnedElements.Remove(type);
            Remove(elem);
            graph.ClosePinned(type);
        }

        public Status GetPinnedElementStatus<T>() where T : PinnedElementView
        {
            return GetPinnedElementStatus(typeof(T));
        }

        public Status GetPinnedElementStatus(Type type)
        {
            var pinned = graph.pinnedElements.Find(p => p.editorTypeFullName == type.FullName);

            if (pinned != null && pinned.opened)
                return Status.Normal;
            else
                return Status.Hidden;
        }

        void RemovePinnedElementViews()
        {
            foreach (var pinnedView in pinnedElements.Values)
            {
                if (Contains(pinnedView))
                    Remove(pinnedView);
            }
            pinnedElements.Clear();
        }

        #endregion

        #region 注释

#if UNITY_2020_1_OR_NEWER
        public StickyNoteView AddStickyNote(StickyNote note)
        {
            graph.AddStickyNote(note);
            return AddStickyNoteView(note);
        }

        public StickyNoteView AddStickyNoteView(StickyNote note)
        {
            var c = new StickyNoteView();

            c.Initialize(this, note);

            AddElement(c);

            stickyNoteViews.Add(c);
            return c;
        }

        public void RemoveStickyNoteView(StickyNoteView view)
        {
            stickyNoteViews.Remove(view);
            RemoveElement(view);
        }

        public void RemoveStrickyNotes()
        {
            foreach (var stickyNodeView in stickyNoteViews)
                RemoveElement(stickyNodeView);
            stickyNoteViews.Clear();
        }
#endif

        #endregion

        #region 断开连接

        public void Disconnect(EdgeView e, bool refreshPorts = true)
        {
            // Remove the serialized edge if there is one
            if (e.userData is BaseEdge serializableEdge)
                graph.DisconnectEdge(serializableEdge.id);

            DisconnectView(e, refreshPorts);

            //UpdateComputeOrder();
        }

        public void DisconnectView(EdgeView e, bool refreshPorts = true)
        {
            if (e == null)
                return;

            RemoveElement(e);

            if (e?.input?.node is BaseNodeView inputNodeView)
            {
                e.input.Disconnect(e);
                if (refreshPorts)
                    inputNodeView.RefreshPorts();
            }
            if (e?.output?.node is BaseNodeView outputNodeView)
            {
                e.output.Disconnect(e);
                if (refreshPorts)
                    outputNodeView.RefreshPorts();
            }

            edgeViews.Remove(e);
        }

        #endregion

        #region 链接

        public bool ConnectView(EdgeView e, bool autoDisconnectInputs = true)
        {
            if (!CanConnectEdge(e, autoDisconnectInputs))
                return false;

            var inputPortView = e.input as PortView;
            var outputPortView = e.output as PortView;
            var inputNodeView = inputPortView.node as BaseNodeView;
            var outputNodeView = outputPortView.node as BaseNodeView;

            //If the input port does not support multi-connection, we remove them
            if (autoDisconnectInputs && !(e.input as PortView).portData.acceptMultipleEdges)
            {
                foreach (var edge in edgeViews.Where(ev => ev.input == e.input).ToList())
                {
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    DisconnectView(edge);
                }
            }
            // same for the output port:
            if (autoDisconnectInputs && !(e.output as PortView).portData.acceptMultipleEdges)
            {
                foreach (var edge in edgeViews.Where(ev => ev.output == e.output).ToList())
                {
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    DisconnectView(edge);
                }
            }

            AddElement(e);

            e.input.Connect(e);
            e.output.Connect(e);

            // If the input port have been removed by the custom port behavior
            // we try to find if it's still here
            if (e.input == null)
                e.input = inputNodeView.GetPortViewFromFieldName(inputPortView.fieldName, inputPortView.portData.id);
            if (e.output == null)
                e.output = inputNodeView.GetPortViewFromFieldName(outputPortView.fieldName, outputPortView.portData.id);

            edgeViews.Add(e);

            inputNodeView.RefreshPorts();
            outputNodeView.RefreshPorts();

            // In certain cases the edge color is wrong so we patch it
            schedule.Execute(() =>
            {
                e.UpdateEdgeControl();
            }).ExecuteLater(1);

            e.isConnected = true;

            return true;
        }

        public bool Connect(PortView inputPortView, PortView outputPortView, bool autoDisconnectInputs = true)
        {
            var inputPort = inputPortView.nodeView.nodeTarget.GetPort(inputPortView.fieldName, inputPortView.portData.id);
            var outputPort = outputPortView.nodeView.nodeTarget.GetPort(outputPortView.fieldName, outputPortView.portData.id);

            // Checks that the node we are connecting still exists
            if (inputPortView.nodeView.parent == null || outputPortView.nodeView.parent == null)
                return false;

            var newEdge = BaseEdge.CreateNewEdge(graph, inputPort, outputPort);

            var edgeView = CreateEdgeView();
            edgeView.userData = newEdge;
            edgeView.input = inputPortView;
            edgeView.output = outputPortView;


            return Connect(edgeView);
        }

        public bool Connect(EdgeView e, bool autoDisconnectInputs = true)
        {
            if (!CanConnectEdge(e, autoDisconnectInputs))
                return false;

            var inputPortView = e.input as PortView;
            var outputPortView = e.output as PortView;
            var inputNodeView = inputPortView.node as BaseNodeView;
            var outputNodeView = outputPortView.node as BaseNodeView;
            var inputPort = inputNodeView.nodeTarget.GetPort(inputPortView.fieldName, inputPortView.portData.id);
            var outputPort = outputNodeView.nodeTarget.GetPort(outputPortView.fieldName, outputPortView.portData.id);

            e.userData = graph.ConnectPort(inputPort, outputPort, autoDisconnectInputs);

            ConnectView(e, autoDisconnectInputs);

            //UpdateComputeOrder();

            return true;
        }

        #endregion

        #region Check

        //选中的节点是否需要在Inspector显示
        bool DoesSelectionContainsInspectorNodes()
            => selection.Any(s => s is BaseNodeView v && v.nodeTarget.needsInspector);

        //是否可以链接边缘
        public bool CanConnectEdge(EdgeView e, bool autoDisconnectInputs = true)
        {
            if (e.input == null || e.output == null)
                return false;

            var inputPortView = e.input as PortView;
            var outputPortView = e.output as PortView;
            var inputNodeView = inputPortView.node as BaseNodeView;
            var outputNodeView = outputPortView.node as BaseNodeView;

            if (inputNodeView == null || outputNodeView == null)
            {
                Debug.LogError("Connect aborted !");
                return false;
            }

            return true;
        }

        #endregion

        #region 数据清理

        public void ClearGraphElements()
        {
            RemoveGroups();
            RemoveNodeViews();
            RemoveEdges();
            RemovePinnedElementViews();
            RemoveStrickyNotes();
        }

        public void Dispose()
        {
            ClearGraphElements();
            RemoveFromHierarchy();

            exposedParameterFactory.Dispose();
            exposedParameterFactory = null;

            graph.onExposedParameterListChanged -= OnExposedParameterListChanged;
            graph.onExposedParameterModified += (s) => onExposedParameterModified?.Invoke(s);
            graph.onGraphChanges -= GraphChangesCallback;
        }

        #endregion

        #region 事件处理

        //参数改变
        void OnExposedParameterListChanged()
        {
            UpdateSerializedProperties();
            onExposedParameterListChanged?.Invoke();
        }

        #endregion

        #region Public

        //注册撤销标记
        public void RegisterCompleteObjectUndo(string name)
        {
            //Undo.RegisterCompleteObjectUndo(graph, name);
        }

        public void ResetPositionAndZoom()
        {
            graph.position = Vector3.zero;
            graph.scale = Vector3.one;

            UpdateViewTransform(graph.position, graph.scale);
        }

        #endregion
    }
}
