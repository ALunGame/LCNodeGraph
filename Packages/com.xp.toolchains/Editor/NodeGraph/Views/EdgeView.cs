using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace XPToolchains.NodeGraph
{
    //端口边缘视图
    public class EdgeView : Edge
    {
        readonly string edgeStyle = "EdgeView";

        public BaseEdge Edge { get { return userData as BaseEdge; } }

        public bool isConnected = false;

        protected BaseGraphView graphView => ((input ?? output) as PortView).nodeView.graphView;

        public EdgeView() : base()
        {
            styleSheets.Add(NodeGraphDefine.LoadUSS(edgeStyle));
        }

        public override void OnPortChanged(bool isInput)
        {
            base.OnPortChanged(isInput);
            UpdateEdgeSize();
        }

        public void UpdateEdgeSize()
        {
            if (input == null && output == null)
                return;

            PortData inputPortData = (input as PortView)?.portData;
            PortData outputPortData = (output as PortView)?.portData;

            for (int i = 1; i < 20; i++)
                RemoveFromClassList($"edge_{i}");
            int maxPortSize = Mathf.Max(inputPortData?.sizeInPixel ?? 0, outputPortData?.sizeInPixel ?? 0);
            if (maxPortSize > 0)
                AddToClassList($"edge_{Mathf.Max(1, maxPortSize - 6)}");
        }

        protected override void OnCustomStyleResolved(ICustomStyle styles)
        {
            base.OnCustomStyleResolved(styles);

            UpdateEdgeControl();
        }
    }
}
