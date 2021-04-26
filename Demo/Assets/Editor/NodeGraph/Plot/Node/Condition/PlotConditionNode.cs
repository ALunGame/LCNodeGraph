using XPToolchains.NodeGraph;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Demo.Plot.Condition
{
    public class ConditionData
    {
    }

    public abstract class ConditionNode : BaseNode
    {
        public override Color color => Color.magenta;

        public abstract string funcName { get; }

        [Output(name = "节点")]
        public ConditionData outPut;
    }

    [NodeMenuItem("条件/是否拥有指定物品")]
    public class ConditionCheckItem : ConditionNode
    {
        public override string name => "检测物品";

        public override string funcName => "CheckItem";

        [Input(name = "物品Id"), ShowAsDrawer]
        public string itemId = "";
        [Input(name = "物品数量"), ShowAsDrawer]
        public string itemCount = "";
        [Input(name = "hasOwn"), ShowAsDrawer]
        public string hasOwn = "";
    }

    [NodeMenuItem("条件/人物位置是否大于指定位置（格子）")]
    public class ConditionCheckCharaCell : ConditionNode
    {
        public override string name => "人物位置是否大于指定位置（格子）";

        public override string funcName => "CheckCharaCell";

        [Input(name = "人物Id"), ShowAsDrawer]
        public string charaId = "";
        [Input(name = "坐标x"), ShowAsDrawer]
        public string x = "";
        [Input(name = "坐标y"), ShowAsDrawer]
        public string y = "";
    }
}
