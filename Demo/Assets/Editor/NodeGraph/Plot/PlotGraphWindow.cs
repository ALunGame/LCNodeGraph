using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using XPToolchains.NodeGraph;

namespace Demo.Plot
{
    /// <summary>
    /// 剧情视图
    /// </summary>
    public class PlotGraphWindow : BaseGraphWindow
    {
        public override string BackVerPath => "Assets/Editor/NodeGraph/Plot/EDData/Back/";

        public override string SavePath => "Assets/Editor/NodeGraph/Plot/EDData/Data/";

        public string LuaSavePath => "../Design/ClientData/SysStoryData.lua";
        //服务器用的
        public string JsonSavePath => "Assets/Editor/StorySystem/Data/Story/";


        [MenuItem("剧情/编辑")]
		public static PlotGraphWindow OpenWithTmpGraph()
		{
			PlotGraphWindow window = GetWindow<PlotGraphWindow>();
			window.InitGraph(new List<string>() { "Demo.Plot" });
            window.titleContent = new GUIContent("剧情编辑");
            window.Show();
			return window;
		}

        private string CreateNodeId(BaseGraph graph)
        {
            int nodeId = 1;
            if (graph.nodes == null || graph.nodes.Count <= 0)
            {
                return nodeId.ToString();
            }
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                if (graph.nodes[i] is PlotNode)
                {
                    int tmpId = int.Parse(graph.nodes[i].id);
                    if (tmpId >= nodeId)
                    {
                        nodeId = tmpId;
                    }
                }
            }
            nodeId += 1;
            return nodeId.ToString();
        }

        protected override void GetNodeId(BaseGraph graph, BaseNode node)
        {
            string nodeId = Guid.NewGuid().ToString();
            //步骤节点
            if (node is PlotNode)
            {
                nodeId = CreateNodeId(graph);
            }
            node.id = nodeId;
        }

        //序列化BaseGraph
        public override void SerializeGraph(Dictionary<string, BaseGraph> graphDict)
        {
            //PlotGraphToLua.SaveLua(LuaSavePath, graphDict);
            //Debug.LogWarning("Lua配置生成成功："+ LuaSavePath);
        }
    }
}
