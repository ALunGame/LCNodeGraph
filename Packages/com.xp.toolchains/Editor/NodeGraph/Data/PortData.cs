using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace XPToolchains.NodeGraph
{
    //节点端口数据
    public class PortData
    {
        public string id;

        //端口显示名
        public string displayName;

        //端口显示类型
        public Type displayType;

        //多端口值
        public object value;

        //端口接受多个连接
        public bool acceptMultipleEdges;

        //端口大小
        public int sizeInPixel;

        //端口提示
        public string tooltip;

        //是不是垂直端口
        public bool vertical;

        //显示抽屉值
        public bool showInputDrawer;

        public bool Equals(PortData other)
        {
            return id == other.id
                && displayName == other.displayName
                && displayType == other.displayType
                && acceptMultipleEdges == other.acceptMultipleEdges
                && sizeInPixel == other.sizeInPixel
                && tooltip == other.tooltip
                && vertical == other.vertical;
        }

        public void CopyFrom(PortData other)
        {
            id = other.id;
            displayName = other.displayName;
            displayType = other.displayType;
            acceptMultipleEdges = other.acceptMultipleEdges;
            sizeInPixel = other.sizeInPixel;
            tooltip = other.tooltip;
            vertical = other.vertical;
        }
    }

    //一个节点端口
    public class NodePort
    {
        public string fieldName;
        public FieldInfo fieldInfo;
        //反射拿到的实例
        public object fieldOwner;

        //端口所在节点
        public BaseNode node;
        public PortData portData;

        //边缘
        List<BaseEdge> edges = new List<BaseEdge>();
        //一个空的委托干啥的呢？？
        public delegate void PushDataDelegate();
        //边缘与发送数据到端口的委托
        Dictionary<BaseEdge, PushDataDelegate> pushDataDelegates = new Dictionary<BaseEdge, PushDataDelegate>();
        //不清楚干啥的，
        //TODO
        List<BaseEdge> edgeWithRemoteCustomIO = new List<BaseEdge>();

        public NodePort(BaseNode node, object fieldOwner, string fieldName, PortData portData)
        {
            this.fieldName = fieldName;
            this.node = node;
            this.portData = portData;
            this.fieldOwner = fieldOwner;

            fieldInfo = fieldOwner.GetType().GetField(
                fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public NodePort(BaseNode node, string fieldName, PortData portData) : this(node, node, fieldName, portData) { }

        //将一个边缘链接到此端口
        public void Add(BaseEdge edge)
        {
            if (!edges.Contains(edge))
                edges.Add(edge);

            PushDataDelegate edgeDelegate = CreatePushDataDelegateForEdge(edge);

            if (edgeDelegate != null)
                pushDataDelegates[edge] = edgeDelegate;
        }

        //创建将数据从输入节点移动到输出节点的委托:
        PushDataDelegate CreatePushDataDelegateForEdge(BaseEdge edge)
        {
            try
            {
                FieldInfo inputField = edge.inputNode.GetType().GetField(edge.inputFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo outputField = edge.outputNode.GetType().GetField(edge.outputFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                Type inType, outType;

#if UNITY_EDITOR
                if (!BaseGraph.TypesAreConnectable(inputField.FieldType, outputField.FieldType))
                {
                    Debug.LogError("创建委托失败，这俩个类型不能都连接 你可以实现 自定义的输入输出" + inputField.FieldType + " to " + outputField.FieldType);
                    return null;
                }
#endif

                Expression inputParamField = Expression.Field(Expression.Constant(edge.inputNode), inputField);
                Expression outputParamField = Expression.Field(Expression.Constant(edge.outputNode), outputField);

                inType = edge.inputPort.portData.displayType ?? inputField.FieldType;
                outType = edge.outputPort.portData.displayType ?? outputField.FieldType;
                outputParamField = Expression.Convert(outputParamField, inputField.FieldType);

                BinaryExpression assign = Expression.Assign(inputParamField, outputParamField);
                return Expression.Lambda<PushDataDelegate>(assign).Compile();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        //删除端口边缘
        public void Remove(BaseEdge edge)
        {
            if (!edges.Contains(edge))
                return;

            pushDataDelegates.Remove(edge);
            edgeWithRemoteCustomIO.Remove(edge);
            edges.Remove(edge);
        }

        //获得端口边缘
        public List<BaseEdge> GetEdges() => edges;

        //通过边缘推送端口的值（这会直接传递数据到链接的端口）
        public void PushData()
        {
            foreach (var pushDataDelegate in pushDataDelegates)
                pushDataDelegate.Value();

            if (edgeWithRemoteCustomIO.Count == 0)
                return;

            //缓存数据
            object ourValue = fieldInfo.GetValue(fieldOwner);
            foreach (var edge in edgeWithRemoteCustomIO)
                edge.passThroughBuffer = ourValue;
        }

        //通过边缘拉取端口的值（这会直接传递数据到链接的端口）
        public void PullData()
        {
            // 检查此端口是否连接到具有自定义输出功能的端口
            if (edgeWithRemoteCustomIO.Count == 0)
                return;

            if (edges.Count > 0)
                fieldInfo.SetValue(fieldOwner, edges.First().passThroughBuffer);
        }

        //重置参数默认值
        public void ResetToDefault()
        {
            // 情况列表，类置空，结构默认
            if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType))
                (fieldInfo.GetValue(fieldOwner) as IList)?.Clear();
            else if (fieldInfo.FieldType.GetTypeInfo().IsClass)
                fieldInfo.SetValue(fieldOwner, null);
            else
            {
                try
                {
                    fieldInfo.SetValue(fieldOwner, Activator.CreateInstance(fieldInfo.FieldType));
                }
                catch { }
            }
        }
    }

    //端口容器
    public abstract class NodePortContainer : List<NodePort>
    {
        protected BaseNode node;

        public NodePortContainer(BaseNode node)
        {
            this.node = node;
        }

        //删除端口边缘
        public void Remove(BaseEdge edge)
        {
            ForEach(p => p.Remove(edge));
        }

        //添加端口边缘
        public void Add(BaseEdge edge)
        {
            string portFieldName = (edge.inputNode == node) ? edge.inputFieldName : edge.outputFieldName;
            string portIdentifier = (edge.inputNode == node) ? edge.inputPortId : edge.outputPortId;

            // Force empty string to null since portIdentifier is a serialized value
            if (String.IsNullOrEmpty(portIdentifier))
                portIdentifier = null;

            var port = this.FirstOrDefault(p =>
            {
                return p.fieldName == portFieldName && p.portData.id == portIdentifier;
            });

            if (port == null)
            {
                Debug.LogError($"链接失败》》》 {edge}");
                return;
            }

            port.Add(edge);
        }
    }

    //输入端口
    public class NodeInputPortContainer : NodePortContainer
    {
        public NodeInputPortContainer(BaseNode node) : base(node) { }

        public void PullDatas()
        {
            ForEach(p => p.PullData());
        }
    }

    //输出端口
    public class NodeOutputPortContainer : NodePortContainer
    {
        public NodeOutputPortContainer(BaseNode node) : base(node) { }

        public void PushDatas()
        {
            ForEach(p => p.PushData());
        }
    }
}
