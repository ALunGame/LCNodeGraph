using System;
using XPToolchains.Json;

namespace XPToolchains.NodeGraph
{
    //连接俩个端口的边缘
    public class BaseEdge
    {
        public string id;

        public string inputNodeGUID;
        public string outputNodeGUID;

        public string inputFieldName;
        public string outputFieldName;

        //端口id
        public string inputPortId;
        public string outputPortId;

        [JsonIgnore]
        BaseGraph graph;

        [JsonIgnore]
        public BaseNode inputNode;

        [JsonIgnore]
        public BaseNode outputNode;

        [JsonIgnore]
        public NodePort inputPort;

        [JsonIgnore]
        public NodePort outputPort;

        //当使用自定义输入/输出函数时，函数使用的临时对象。
        [JsonIgnore]
        public object passThroughBuffer;

        public BaseEdge() { }

        //初始化
        public void Init(BaseGraph baseGraph)
        {
            graph = baseGraph;
            if (!graph.nodesPerGUID.ContainsKey(outputNodeGUID) || !graph.nodesPerGUID.ContainsKey(inputNodeGUID))
                return;

            outputNode = graph.nodesPerGUID[outputNodeGUID];
            inputNode = graph.nodesPerGUID[inputNodeGUID];
            inputPort = inputNode.GetPort(inputFieldName, inputPortId);
            outputPort = outputNode.GetPort(outputFieldName, outputPortId);
        }

        public override string ToString() => $"{outputNode.name}:{outputPort.fieldName} -> {inputNode.name}:{inputPort.fieldName}";

        public void InitCopy(BaseGraph graph)
        {
            this.graph = graph;
        }

        //创建边缘
        public static BaseEdge CreateNewEdge(BaseGraph graph, NodePort inputPort, NodePort outputPort)
        {
            BaseEdge edge = new BaseEdge();

            edge.graph = graph;
            edge.id = Guid.NewGuid().ToString();
            edge.inputNode = inputPort.node;
            edge.inputFieldName = inputPort.fieldName;
            edge.outputNode = outputPort.node;
            edge.outputFieldName = outputPort.fieldName;
            edge.inputPort = inputPort;
            edge.outputPort = outputPort;

            edge.inputPortId = inputPort.portData.id;
            edge.outputPortId = outputPort.portData.id;

            edge.outputNodeGUID = edge.outputNode.id;
            edge.inputNodeGUID = edge.inputNode.id;

            return edge;
        }
    }
}
