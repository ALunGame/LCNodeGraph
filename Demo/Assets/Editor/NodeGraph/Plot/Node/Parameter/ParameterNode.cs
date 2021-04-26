using XPToolchains.NodeGraph;

namespace Demo.Plot.Parameter
{
    public class ParameterData
    {
    }
    public class ParameterNode : BaseNode
    {
        [Output(name = "步骤")]
        public ParameterData outPut;
    }

    [NodeMenuItem("参数/节点参数")]
    public class BaseParameterNode : ParameterNode
    {
        public override string name => "节点参数";

        [Input(name = "容错跳转"), ShowAsDrawer]
        public string backTo = "";
    }
}
