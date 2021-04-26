using System.IO;
using UnityEngine.UIElements;

namespace XPToolchains.NodeGraph
{
    /// <summary>
    /// 视图列表窗口
    /// </summary>
    public class GraphListView : VisualElement
    {
        static readonly string pinnedElementTree = "GraphListElement";
        static readonly string pinnedElementStyle = "GraphList";
        private ScrollView scrollView;

        public GraphListView()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            var xml = NodeGraphDefine.LoadUXML(pinnedElementTree);
            styleSheets.Add(NodeGraphDefine.LoadUSS(pinnedElementStyle));
            xml.CloneTree(this);

            //滑动条
            scrollView = this.Q<ScrollView>(name: "scroll");
        }

        public void Init(BaseGraphWindow graphWindow)
        {
            scrollView.Clear();
            foreach (var item in graphWindow.graphDict)
            {
                string filePath = item.Key;
                FileInfo graphFileInfo = new FileInfo(filePath);
                Button btn = new Button(() =>
                {
                    graphWindow.ChangeGraph(filePath);
                });
                btn.text = graphFileInfo.Name.Replace(graphFileInfo.Extension,"");
                if (graphWindow.selGraphPath== filePath)
                    btn.AddToClassList("btn_sel");
                else
                    btn.AddToClassList("btn_noSel");

                scrollView.Add(btn);
            }
        }
    }
}
