using XPToolchains.NodeGraph;
using UnityEngine;

namespace Demo.Plot.Trigger
{
    public class TriggerData
    {
    }

    public abstract class TriggerNode : BaseNode
    {
        public override Color color => Color.red;
        public abstract string funcName { get; }

        [Output(name = "节点")]
        public TriggerData outPut;
    }

    [NodeMenuItem("触发/进入地图")]
    public class TriggerEnterMap : TriggerNode
    {
        public override string name => "进入地图";

        public override string funcName => "EnterMap";
    }

    [NodeMenuItem("触发/点击气泡")]
    public class TriggerClickBubble : TriggerNode
    {
        public override string name => "点击气泡";

        public override string funcName => "ClickBubble";

        [Input(name = "人物Id"), ShowAsDrawer]
        public string charaId = "";
    }

    [NodeMenuItem("触发/完成所有前置节点")]
    public class TriggerFinishAllPrevNodes : TriggerNode
    {
        public override string name => "完成所有前置节点";

        public override string funcName => "FinishAllPrevNodes";
    }

    [NodeMenuItem("触发/主角移动结束")]
    public class TriggerMainCharaMoveEnd : TriggerNode
    {
        public override string name => "主角移动结束";

        public override string funcName => "MainCharaMoveEnd";
    }

    [NodeMenuItem("触发/点击物品")]
    public class TriggerClickItem : TriggerNode
    {
        public override string name => "点击物品";

        public override string funcName => "ClickItem";

        [Input(name = "物品Id"), ShowAsDrawer]
        public string itemId = "";
    }
}
