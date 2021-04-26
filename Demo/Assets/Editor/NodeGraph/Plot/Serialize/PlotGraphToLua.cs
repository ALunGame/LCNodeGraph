using Demo.Plot.Action;
using Demo.Plot.Condition;
using Demo.Plot.Parameter;
using Demo.Plot.Trigger;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XPToolchains.NodeGraph;

namespace Demo.Plot
{
    //因为 Lua序列化不支持属性忽略，只能创建模板
    public class PlotGraph
    {
        public string name;
        public Dictionary<int, PlotStep> nodes = new Dictionary<int, PlotStep>();
    }

    public class PlotStep
    {
        public int id;

        public int nodeType;
        public Dictionary<string, string> nodeParam = new Dictionary<string, string>();

        public List<LuaNodeFunc> triggers = new List<LuaNodeFunc>();
        public List<LuaNodeFunc> conditions = new List<LuaNodeFunc>();
        public List<LuaNodeFunc> acts = new List<LuaNodeFunc>();

        //上一个步骤
        public List<int> prevNodes = new List<int>();
        //下一个步骤
        public List<int> nextNodes = new List<int>();
    }

    public class LuaNodeFunc
    {
        public string name = "";
        public Dictionary<string, string> param = new Dictionary<string, string>();
    }


    public static class PlotGraphToLua
    {
        //兼容旧的配置
        private static Dictionary<string, Dictionary<string, string>> CustomNodeFileFixDict = new Dictionary<string, Dictionary<string, string>>()
        {
            {"PlayMiniGame",new Dictionary<string, string>(){ { "gameName", "name" }, { "gameId", "id" }, } },
            {"PlayEffect",new Dictionary<string, string>(){ { "effectName", "name" }, { "effectType", "type" }, } },
        };

        //获得这个节点的输出节点
        public static List<BaseNode> GetNodeOutNodes(BaseGraph baseGraph, BaseNode checkNode)
        {
            List<BaseNode> childNodes = new List<BaseNode>();
            for (int i = 0; i < baseGraph.edges.Count; i++)
            {
                BaseEdge baseEdge = baseGraph.edges[i];
                if (baseEdge.outputNodeGUID == checkNode.id)
                {
                    BaseNode inputNode = baseGraph.nodes.First(v => v.id == baseEdge.inputNodeGUID);
                    if (inputNode!=null)
                    {
                        childNodes.Add(inputNode);
                    }
                }
            }
            return childNodes;
        }

        //获得这个节点的输入节点
        public static List<BaseNode> GetNodeInNodes(BaseGraph baseGraph,BaseNode checkNode)
        {
            List<BaseNode> childNodes = new List<BaseNode>();
            for (int i = 0; i < baseGraph.edges.Count; i++)
            {
                BaseEdge baseEdge = baseGraph.edges[i];
                if (baseEdge.inputNodeGUID == checkNode.id)
                {
                    BaseNode outputNode = baseGraph.nodes.FirstOrDefault(v => v.id == baseEdge.outputNodeGUID);
                    if (outputNode!=null)
                    {
                        childNodes.Add(outputNode);
                    }
                }
            }
            return childNodes;
        }

        //创建LuaNodeFunc
        private static LuaNodeFunc CreateLuaNodeFunc(BaseNode baseNode)
        {
            LuaNodeFunc nodeFunc = new LuaNodeFunc();

            MethodInfo funcNameFunc = baseNode.GetType().GetMethod("get_funcName");
            string funcName = funcNameFunc.Invoke(baseNode,null).ToString();
            nodeFunc.name = funcName;

            //设置参数
            foreach (FieldInfo fInfo in baseNode.GetType().GetFields())
            {
                InputAttribute inputAttr = (InputAttribute)fInfo.GetCustomAttribute(typeof(InputAttribute));
                if (inputAttr==null)
                    continue;

                string value = fInfo.GetValue(baseNode).ToString();
                string valueName = fInfo.Name;
                if (CustomNodeFileFixDict.ContainsKey(nodeFunc.name) && CustomNodeFileFixDict[nodeFunc.name].ContainsKey(valueName))
                {
                    valueName = CustomNodeFileFixDict[nodeFunc.name][valueName];
                }
                nodeFunc.param.Add(valueName, value);
            }

            return nodeFunc;
        }

        //创建节点参数
        private static Dictionary<string, string> CreateNodeParam(ParameterNode parameterNode)
        {
            Dictionary<string, string> paramDict = new Dictionary<string, string>();
            //设置参数
            foreach (FieldInfo fInfo in parameterNode.GetType().GetFields())
            {
                InputAttribute inputAttr = (InputAttribute)fInfo.GetCustomAttribute(typeof(InputAttribute));
                if (inputAttr == null)
                    continue;

                string value = fInfo.GetValue(parameterNode).ToString();
                string valueName = fInfo.Name;
                paramDict.Add(valueName, value);
            }
            return paramDict;
        }

        private static Dictionary<int, PlotStep> CreatePlotSteps(BaseGraph baseGraph)
        {
            PlotStep genPlotStep(PlotNode plotNode, BaseGraph graph)
            {
                PlotStep plotStep = new PlotStep();
                plotStep.id = int.Parse(plotNode.id);
                plotStep.nodeType = (int)plotNode.nodeType;

                List<BaseNode> inputNodes = GetNodeInNodes(graph, plotNode);
                for (int i = 0; i < inputNodes.Count; i++)
                {
                    BaseNode childNode = inputNodes[i];
                    if (childNode is ActionNode)
                    {
                        plotStep.acts.Add(CreateLuaNodeFunc(childNode));
                    }
                    else if (childNode is ConditionNode)
                    {
                        plotStep.conditions.Add(CreateLuaNodeFunc(childNode));
                    }
                    else if (childNode is TriggerNode)
                    {
                        plotStep.triggers.Add(CreateLuaNodeFunc(childNode));
                    }
                    else if (childNode is ParameterNode)
                    {
                        plotStep.nodeParam = CreateNodeParam((ParameterNode)childNode);
                    }
                }

                return plotStep;
            }

            //生成步骤
            Dictionary<int, PlotStep> plotStepDict = new Dictionary<int, PlotStep>();
            Dictionary<int, PlotNode> plotNodeDict = new Dictionary<int, PlotNode>();
            for (int i = 0; i < baseGraph.nodes.Count; i++)
            {
                BaseNode baseNode = baseGraph.nodes[i];
                if (baseNode is PlotNode)
                {
                    plotNodeDict.Add(int.Parse(baseNode.id), (PlotNode)baseNode);
                    plotStepDict.Add(int.Parse(baseNode.id), genPlotStep((PlotNode)baseNode, baseGraph));
                }
            }

            //创建连接关系
            foreach (var item in plotStepDict)
            {
                PlotStep plotStep = item.Value;
                PlotNode plotNode = plotNodeDict[plotStep.id];

                List<BaseNode> inputNodes = GetNodeInNodes(baseGraph, plotNode);
                for (int i = 0; i < inputNodes.Count; i++)
                {
                    if (inputNodes[i] is PlotNode)
                    {
                        plotStep.prevNodes.Add(int.Parse(inputNodes[i].id));
                    }
                }

                List<BaseNode> outputNodes = GetNodeOutNodes(baseGraph, plotNode);
                for (int i = 0; i < outputNodes.Count; i++)
                {
                    if (outputNodes[i] is PlotNode)
                    {
                        plotStep.nextNodes.Add(int.Parse(outputNodes[i].id));
                    }
                }
            }

            return plotStepDict;
        }

        private static PlotGraph ToPlotGraph(BaseGraph baseGraph)
        {
            PlotGraph plotGraph = new PlotGraph();
            plotGraph.name = baseGraph.displayName;
            plotGraph.nodes = CreatePlotSteps(baseGraph);

            return plotGraph;
        }

        public static Dictionary<string, PlotGraph> SaveLua(string savePath, Dictionary<string, BaseGraph> graphDict)
        {
            Dictionary<string, PlotGraph> luaDict = new Dictionary<string, PlotGraph>();
            foreach (var item in graphDict)
            {
                string name = item.Key;
                BaseGraph baseGraph = item.Value;
                luaDict.Add(baseGraph.displayName, ToPlotGraph(baseGraph));
            }

            //XPTools.DoExcel.OutFile file = new XPTools.DoExcel.OutFile(string.Empty, savePath);
            //file.WriteTableDef("SysStoryData");
            //foreach (KeyValuePair<string, PlotGraph> it in luaDict)
            //{
            //    file.WriteTableKey("SysStoryData", it.Key, it.Value);
            //}

            return luaDict;
        }
    }
}
