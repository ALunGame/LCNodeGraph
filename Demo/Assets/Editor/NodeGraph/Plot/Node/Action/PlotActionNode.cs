using XPToolchains.NodeGraph;
using UnityEngine;

namespace Demo.Plot.Action
{
    public class ActionData
    {
    }

    public abstract class ActionNode : BaseNode
    {
        public override Color color => Color.green;

        public abstract string funcName { get; }

        [Output(name = "节点")]
        public ActionData outPut;
    }

    [NodeMenuItem("行为/对话")]
    public class ActionPlayDialog : ActionNode
    {
        public override string name => "对话";

        public override string funcName => "PlayDialog";

        [Input(name = "对话Id"), ShowAsDrawer]
        public string dialogId = "";
        [Input(name = "对话类型"), ShowAsDrawer]
        public string typeId = "";
        [Input(name = "背景图片"), ShowAsDrawer]
        public string picName = "";
    }

    [NodeMenuItem("行为/玩游戏")]
    public class ActionPlayGame : ActionNode
    {
        public override string name => "玩游戏";

        public override string funcName => "PlayMiniGame";

        [Input(name = "游戏名"), ShowAsDrawer]
        public string gameName = "";

        [Input(name = "游戏名"), ShowAsDrawer]
        public string gameId = "";
    }

    [NodeMenuItem("行为/播放效果")]
    public class ActionPlayEffect : ActionNode
    {
        public override string name => "播放效果";

        public override string funcName => "PlayEffect";

        [Input(name = "效果名"), ShowAsDrawer]
        public string effectName = "";
        [Input(name = "效果类型"), ShowAsDrawer]
        public string effectType = "";
        [Input(name = "点击结束"), ShowAsDrawer]
        public string endByClick = "";
        [Input(name = "结束时间"), ShowAsDrawer]
        public string endTime = "";
    }

    [NodeMenuItem("行为/下一个地图")]
    public class ActionGoNextStory : ActionNode
    {
        public override string name => "下一个地图";

        public override string funcName => "GoNextStory";

        [Input(name = "对话Id"), ShowAsDrawer]
        public string storyId = "";
    }

    [NodeMenuItem("行为/显示气泡")]
    public class ActionPlayBubble : ActionNode
    {
        public override string name => "显示气泡";

        public override string funcName => "PlayBubble";

        [Input(name = "人物Id"), ShowAsDrawer]
        public string charaId = "";
        [Input(name = "气泡类型"), ShowAsDrawer]
        public string bubbleType = "";
        [Input(name = "对话Id"), ShowAsDrawer]
        public string dialogId = "";
        [Input(name = "对话步骤"), ShowAsDrawer]
        public string dialogStep = "";
    }

    [NodeMenuItem("行为/人物移动到指定格子")]
    public class ActionCharaMoveToCell : ActionNode
    {
        public override string name => "人物移动到指定格子";

        public override string funcName => "CharaMoveToCell";

        [Input(name = "人物Id"), ShowAsDrawer]
        public string charaId = "";
        [Input(name = "坐标x"), ShowAsDrawer]
        public string x = "";
        [Input(name = "坐标y"), ShowAsDrawer]
        public string y = "";
    }

    [NodeMenuItem("行为/玩家移动到NPC")]
    public class ActionPlayerMoveToActor : ActionNode
    {
        public override string name => "玩家移动到NPC";

        public override string funcName => "PlayerMoveToActor";

        [Input(name = "人物Id"), ShowAsDrawer]
        public string charaId = "";
    }

    [NodeMenuItem("行为/对话并返回上一步")]
    public class ActionPlayDialogAndBack : ActionNode
    {
        public override string name => "对话并返回上一步";

        public override string funcName => "PlayDialogAndBack";

        [Input(name = "对话Id"), ShowAsDrawer]
        public string dialogId = "";
        [Input(name = "对话类型"), ShowAsDrawer]
        public string typeId = "";
        [Input(name = "背景图片"), ShowAsDrawer]
        public string picName = "";
    }

     [NodeMenuItem("行为/跳转到指定节点")]
    public class ActionJumpToSpecificNode : ActionNode
    {
        public override string name => "跳转到指定节点";

        public override string funcName => "JumpToSpecificNode";

        [Input(name = "节点Id"), ShowAsDrawer]
        public string nodeId = "";
    }

    [NodeMenuItem("行为/切换动画")]
    public class ActionChangeAnim : ActionNode
    {
        public override string name => "切换动画";

        public override string funcName => "ChangeAnim";

        [Input(name = "人物Id"), ShowAsDrawer]
        public string charaId = "";
        [Input(name = "动画状态"), ShowAsDrawer]
        public string animState = "";
    }

    [NodeMenuItem("行为/显隐人物")]
    public class ActionActiveChara : ActionNode
    {
        public override string name => "显隐人物";

        public override string funcName => "ActiveChara";

        [Input(name = "人物Id"), ShowAsDrawer]
        public string charaId = "";
        [Input(name = "是否显示"), ShowAsDrawer]
        public string isActive = "";
    }

    [NodeMenuItem("行为/显隐物品")]
    public class ActionActiveItem : ActionNode
    {
        public override string name => "显隐物品";

        public override string funcName => "ActiveItem";

        [Input(name = "物品Id"), ShowAsDrawer]
        public string itemId = "";
        [Input(name = "是否显示"), ShowAsDrawer]
        public string isActive = "";
        [Input(name = "坐标x"), ShowAsDrawer]
        public string x = "";
        [Input(name = "坐标y"), ShowAsDrawer]
        public string y = "";
        [Input(name = "特效"), ShowAsDrawer]
        public string effectOn = "";
    }

    [NodeMenuItem("行为/获得物品")]
    public class ActionGetItem : ActionNode
    {
        public override string name => "获得物品";

        public override string funcName => "GetItem";

        [Input(name = "物品Id"), ShowAsDrawer]
        public string itemId = "";
        [Input(name = "物品数量"), ShowAsDrawer]
        public string count = "";
    }

    [NodeMenuItem("行为/设置地图可操作物数据")]
    public class ActionSetOperationData : ActionNode
    {
        public override string name => "设置地图可操作物数据";

        public override string funcName => "SetOperationData";

        [Input(name = "坐标x"), ShowAsDrawer]
        public string x = "";
        [Input(name = "坐标y"), ShowAsDrawer]
        public string y = "";
        [Input(name = "类型"), ShowAsDrawer]
        public string type = "";
        [Input(name = "物品Id"), ShowAsDrawer]
        public string itemId = "";
        [Input(name = "物品数量"), ShowAsDrawer]
        public string itemCount = "";
    }

    [NodeMenuItem("行为/根据条件分发行为")]
    public class ActionCondToAction : ActionNode
    {
        public override string name => "根据条件分发行为";

        public override string funcName => "CondToAction";

        [Input(name = "条件枚举(,分隔)"), ShowAsDrawer]
        public string condEnum = "";
    }
}
