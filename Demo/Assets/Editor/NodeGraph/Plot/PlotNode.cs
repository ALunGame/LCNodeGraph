using Demo.Plot.Action;
using Demo.Plot.Condition;
using Demo.Plot.Trigger;
using XPToolchains.NodeGraph;
using UnityEngine;
using Demo.Plot.Parameter;

namespace Demo.Plot
{
    //没有具体意义，只是为了类型匹配
    public class PlotNodeData
    {

    }

    public enum PlotNodeType
    {
        MainPlot = 1,       // 主线
        BranchPlot = 2,     // 支线
        PersistedNode = 3,  // 常驻节点
    }

    [NodeMenuItem("步骤")]
    public class PlotNode : BaseNode
    {
        public override string name => "步骤" + nodeDes;

        [Input(name = "上一步骤", allowMultiple = true)]
        public PlotNodeData preNodes = null;

        [Input(name = "触发", allowMultiple = true)]
        public TriggerData triggers = null;

        [Input(name = "条件", allowMultiple = true)]
        public ConditionData conditions = null;

        [Input(name = "行为", allowMultiple = true)]
        public ActionData actions = null;

        [Input(name = "参数", allowMultiple = false)]
        public ParameterData param = null;

        [Output(name = "下一步骤", allowMultiple = true)]
        public PlotNodeData nextNodes = null;

        [Output(name = "节点类型"), ShowAsDrawer]
        public PlotNodeType nodeType = PlotNodeType.MainPlot;

        [Output(name = "节点注释"), ShowAsDrawer]
        public string nodeDes = "";
    }

}
