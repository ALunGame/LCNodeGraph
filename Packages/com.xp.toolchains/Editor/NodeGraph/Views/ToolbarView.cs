using System;
using UnityEngine.UIElements;

namespace XPToolchains.NodeGraph
{
    //上方工具条
    public class ToolbarView : VisualElement
    {
        static readonly string pinnedElementTree = "ToolbarElement";
        static readonly string pinnedElementStyle = "Toolbar";

        private ScrollView scrollView;
        private Button listBtn;
        private Button rollBackBtn;
        private BaseGraphWindow mGraphWindow;

        public ToolbarView()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            var xml = NodeGraphDefine.LoadUXML(pinnedElementTree);
            xml.CloneTree(this);
            styleSheets.Add(NodeGraphDefine.LoadUSS(pinnedElementStyle));

            //滑动条
            scrollView = this.Q<ScrollView>(name: "scroll");
        }

        private Button CreateBtn(string name, Action callBack)
        {
            Button button = new Button(callBack);
            button.text = name;
            scrollView.Add(button);
            return button;
        }

        public void Init(BaseGraphWindow graphWindow, BaseGraphView graphView)
        {
            mGraphWindow = graphWindow;
            scrollView.Clear();

            if (graphView != null)
            {
                Label label = new Label("当前视图：" + graphView.graph.displayName);
                scrollView.Add(label);
            }

            if (graphView != null)
            {
                CreateBtn("回到中心", graphView.ResetPositionAndZoom);
            }

            listBtn = CreateBtn("视图列表", () =>
            {
                bool showGraphListView = graphWindow.showGraphList;
                if (showGraphListView)
                {
                    graphWindow.showGraphList = false;
                    graphWindow.graphListView.visible = false;
                }
                else
                {
                    graphWindow.showGraphList = true;
                    graphWindow.graphListView.visible = true;
                }
                RefreshBtnSel();
            });

            CreateBtn("生成视图Lua", () => { graphWindow.SerializeGraph(graphWindow.graphDict); });

            CreateBtn("新建视图", () => { graphWindow.AddGraph(); });

            CreateBtn("删除当前视图", () => { graphWindow.DelGraph(); });

            rollBackBtn = CreateBtn("版本回退", () =>
            {
                bool showRollback = graphWindow.showRollback;
                if (showRollback)
                {
                    graphWindow.showRollback = false;
                    graphWindow.rollbackView.visible = false;
                }
                else
                {
                    graphWindow.showRollback = true;
                    graphWindow.rollbackView.visible = true;
                }
                RefreshBtnSel();
            });
        }

        private void RefreshBtnSel()
        {
            listBtn.AddToClassList(mGraphWindow.showGraphList ? "btn_sel" : "btn_noSel");
            rollBackBtn.AddToClassList(mGraphWindow.showRollback ? "btn_sel" : "btn_noSel");
        }
    }
}
