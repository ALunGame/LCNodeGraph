using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace XPToolchains.NodeGraph
{
    //端口视图
    public class PortView : Port
    {
        readonly string portStyle = "PortView";

        public PortData portData;

        public string fieldName => fieldInfo.Name;
        public Type fieldType => fieldInfo.FieldType;
        public FieldInfo fieldInfo;

        public BaseNodeView nodeView { get; private set; }

        public int ConnectionCount => edges.Count;

        protected BaseEdgeConnectorListener listener;

        //隐藏继承的成员
        public new Type portType;

        //事件
        public event Action<PortView, Edge> OnConnected;
        public event Action<PortView, Edge> OnDisconnected;

        //端口边缘
        private List<EdgeView> edges = new List<EdgeView>();


        public PortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener edgeConnectorListener)
            : base(portData.vertical ? Orientation.Vertical : Orientation.Horizontal, direction, Capacity.Multi, portData.displayType ?? fieldInfo.FieldType)
        {
            //加载样式表
            styleSheets.Add(NodeGraphDefine.LoadUSS(portStyle));

            this.fieldInfo = fieldInfo;
            listener = edgeConnectorListener;
            portType = portData.displayType ?? fieldInfo.FieldType;
            this.portData = portData;
            portName = fieldName;

            //悬浮提示
            tooltip = portData.tooltip;

            if (portData.vertical)
                AddToClassList("Vertical");
        }

        public virtual void Init(BaseNodeView nodeView, string name)
        {
            this.nodeView = nodeView;
            AddToClassList(fieldName);

            //如果端口接受多个值
            if (direction == Direction.Input && portData.acceptMultipleEdges && portType == fieldType)
            {
                if (fieldType.GetGenericArguments().Length > 0)
                    portType = fieldType.GetGenericArguments()[0];
            }

            if (name != null)
                portName = name;

            visualClass = "Port_" + portType.Name;
            tooltip = portData.tooltip;
        }

        #region public

        public List<EdgeView> GetEdges()
        {
            return edges;
        }

        public override void Connect(Edge edge)
        {
            OnConnected?.Invoke(this, edge);

            base.Connect(edge);

            var inputNode = (edge.input as PortView).nodeView;
            var outputNode = (edge.output as PortView).nodeView;

            edges.Add(edge as EdgeView);

            inputNode.OnPortConnected(edge.input as PortView);
            outputNode.OnPortConnected(edge.output as PortView);
        }

        public override void Disconnect(Edge edge)
        {
            OnDisconnected?.Invoke(this, edge);

            base.Disconnect(edge);

            if (!(edge as EdgeView).isConnected)
                return;

            var inputNode = (edge.input as PortView).nodeView;
            var outputNode = (edge.output as PortView).nodeView;

            inputNode.OnPortDisconnected(edge.input as PortView);
            outputNode.OnPortDisconnected(edge.output as PortView);

            edges.Remove(edge as EdgeView);
        }

        #endregion

        #region 更新

        public void UpdatePortView(PortData data)
        {
            if (data.displayType != null)
            {
                base.portType = data.displayType;
                portType = data.displayType;
                visualClass = "Port_" + portType.Name;
            }
            if (!String.IsNullOrEmpty(data.displayName))
                base.portName = data.displayName;

            portData = data;

            //更新边缘
            schedule.Execute(() =>
            {
                foreach (var edge in edges)
                {
                    edge.UpdateEdgeControl();
                    edge.MarkDirtyRepaint();
                }
            }).ExecuteLater(50); // Hummm

            UpdatePortSize();
        }

        //更新端口视图的大小
        public void UpdatePortSize()
        {
            int size = portData.sizeInPixel == 0 ? 8 : portData.sizeInPixel;
            var connector = this.Q("connector");
            var cap = connector.Q("cap");
            connector.style.width = size;
            connector.style.height = size;
            cap.style.width = size - 4;
            cap.style.height = size - 4;

            // 更新端口区域
            edges.ForEach(e => e.UpdateEdgeSize());
        }

        #endregion

        #region public static

        public static PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener edgeConnectorListener)
        {
            var pv = new PortView(direction, fieldInfo, portData, edgeConnectorListener);
            pv.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
            pv.AddManipulator(pv.m_EdgeConnector);

            // Force picking in the port label to enlarge the edge creation zone
            var portLabel = pv.Q("type");
            if (portLabel != null)
            {
                portLabel.pickingMode = PickingMode.Position;
                portLabel.style.flexGrow = 1;
            }

            // hide label when the port is vertical
            if (portData.vertical && portLabel != null)
                portLabel.style.display = DisplayStyle.None;

            // Fixup picking mode for vertical top ports
            if (portData.vertical)
                pv.Q("connector").pickingMode = PickingMode.Position;

            return pv;
        }

        #endregion
    }
}
