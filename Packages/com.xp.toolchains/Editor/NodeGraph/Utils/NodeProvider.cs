using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace XPToolchains.NodeGraph
{
    //视图中节点辅助
    public class NodeProvider
    {
        //端口描述
        public struct PortDescription
        {
            public Type nodeType;
            public Type portType;
            public bool isInput;
            public string portFieldName;
            public string portIdentifier;
            public string portDisplayName;
        }

        //节点描述
        public class NodeDescriptions
        {
            public Dictionary<string, Type> nodePerMenuTitle = new Dictionary<string, Type>();
            public List<Type> slotTypes = new List<Type>();
            public List<PortDescription> nodeCreatePortDescription = new List<PortDescription>();
        }
        static Dictionary<BaseGraph, NodeDescriptions> specificNodeDescriptions = new Dictionary<BaseGraph, NodeDescriptions>();

        static Dictionary<Type, MonoScript> nodeViewScripts = new Dictionary<Type, MonoScript>();
        static Dictionary<Type, MonoScript> nodeScripts = new Dictionary<Type, MonoScript>();
        static Dictionary<Type, Type> nodeViewPerType = new Dictionary<Type, Type>();
        static NodeDescriptions genericNodes = new NodeDescriptions();

        static NodeProvider()
        {
            BuildScriptCache();
            BuildGenericNodeCache();
        }

        #region 初始化

        static void BuildScriptCache()
        {
            foreach (var nodeType in TypeCache.GetTypesDerivedFrom<BaseNode>())
            {
                if (!IsNodeAccessibleFromMenu(nodeType))
                    continue;

                AddNodeScriptAsset(nodeType);
            }

            foreach (var nodeViewType in TypeCache.GetTypesDerivedFrom<BaseNodeView>())
            {
                if (!nodeViewType.IsAbstract)
                    AddNodeViewScriptAsset(nodeViewType);
            }
        }

        static void BuildGenericNodeCache()
        {
            foreach (var nodeType in TypeCache.GetTypesDerivedFrom<BaseNode>())
            {
                if (!IsNodeAccessibleFromMenu(nodeType))
                    continue;

                BuildCacheForNode(nodeType, genericNodes);
            }
        }


        #endregion

        #region Check

        //节点是否可以从菜单中访问
        static bool IsNodeAccessibleFromMenu(Type nodeType)
        {
            if (nodeType.IsAbstract)
                return false;

            return nodeType.GetCustomAttributes<NodeMenuItemAttribute>().Count() > 0;
        }

        #endregion

        #region Add

        //添加自定义的节点视图绘制
        static void AddNodeViewScriptAsset(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(NodeCustomEditor), false) as NodeCustomEditor[];

            if (attrs != null && attrs.Length > 0)
            {
                Type nodeType = attrs.First().nodeType;
                nodeViewPerType[nodeType] = type;

                var nodeViewScriptAsset = FindScriptFromClassName(type.Name);
                if (nodeViewScriptAsset == null)
                    nodeViewScriptAsset = FindScriptFromClassName(type.Name + "View");
                if (nodeViewScriptAsset == null)
                    nodeViewScriptAsset = FindScriptFromClassName(type.Name + "NodeView");

                if (nodeViewScriptAsset != null)
                    nodeViewScripts[type] = nodeViewScriptAsset;
            }
        }

        static void AddNodeScriptAsset(Type type)
        {
            var nodeScriptAsset = FindScriptFromClassName(type.Name);

            // Try find the class name with Node name at the end
            if (nodeScriptAsset == null)
                nodeScriptAsset = FindScriptFromClassName(type.Name + "Node");
            if (nodeScriptAsset != null)
                nodeScripts[type] = nodeScriptAsset;
        }

        #endregion

        #region private

        static void BuildCacheForNode(Type nodeType, NodeDescriptions targetDescription, BaseGraph graph = null)
        {
            var attrs = nodeType.GetCustomAttributes(typeof(NodeMenuItemAttribute), false) as NodeMenuItemAttribute[];

            if (attrs != null && attrs.Length > 0)
            {
                foreach (var attr in attrs)
                    targetDescription.nodePerMenuTitle[attr.menuTitle] = nodeType;
            }

            foreach (var field in nodeType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.GetCustomAttribute<HideInInspector>() == null && field.GetCustomAttributes().Any(c => c is InputAttribute || c is OutputAttribute))
                    targetDescription.slotTypes.Add(field.FieldType);
            }

            ProvideNodePortCreationDescription(nodeType, targetDescription, graph);
        }

        static FieldInfo SetGraph = typeof(BaseNode).GetField("graph");
        //提供节点端口创建说明
        static void ProvideNodePortCreationDescription(Type nodeType, NodeDescriptions targetDescription, BaseGraph graph = null)
        {
            var node = Activator.CreateInstance(nodeType) as BaseNode;
            try
            {
                SetGraph.SetValue(node, graph);
                node.InitPorts();
            }
            catch (Exception e)
            {

                Debug.LogError($"提供节点端口创建说明>>> {e}");



            }

            foreach (var p in node.inputPorts)
                AddPort(p, true);
            foreach (var p in node.outputPorts)
                AddPort(p, false);

            void AddPort(NodePort p, bool input)
            {
                targetDescription.nodeCreatePortDescription.Add(new PortDescription
                {
                    nodeType = nodeType,
                    portType = p.portData.displayType ?? p.fieldInfo.FieldType,
                    isInput = input,
                    portFieldName = p.fieldName,
                    portDisplayName = p.portData.displayName ?? p.fieldName,
                    portIdentifier = p.portData.id,
                });
            }
        }

        #endregion

        #region public

        public static Type GetNodeViewTypeFromType(Type nodeType)
        {
            Type view;

            if (nodeViewPerType.TryGetValue(nodeType, out view))
                return view;

            foreach (var type in nodeViewPerType)
            {
                if (nodeType.IsSubclassOf(type.Key))
                    return type.Value;
            }

            return view;
        }

        public static IEnumerable<(string path, Type type)> GetNodeMenuEntries(BaseGraph graph = null)
        {
            foreach (var node in genericNodes.nodePerMenuTitle)
                yield return (node.Key, node.Value);
        }

        public static MonoScript GetNodeScript(Type type)
        {
            nodeScripts.TryGetValue(type, out var script);

            return script;
        }

        public static IEnumerable<Type> GetSlotTypes(BaseGraph graph = null)
        {
            foreach (var type in genericNodes.slotTypes)
                yield return type;

        }

        public static IEnumerable<PortDescription> GetEdgeCreationNodeMenuEntry(PortView portView, BaseGraph graph, BaseGraphWindow baseGraphWindow)
        {
            foreach (var description in genericNodes.nodeCreatePortDescription)
            {
                if (!IsPortCompatible(description))
                    continue;

                yield return description;
            }

            bool IsPortCompatible(PortDescription description)
            {
                if (portView.direction == Direction.Input && description.isInput || portView.direction == Direction.Output && !description.isInput)
                    return false;

                if (!BaseGraph.TypesAreConnectable(description.portType, portView.portType))
                    return false;

                return true;
            }
        }

        public static bool FilterNodeByNameSpace(Type nodeType, BaseGraphWindow baseGraphWindow)
        {
            string nameSpace = nodeType.Namespace;
            if (nameSpace == null)
                return true;

            bool isContain = baseGraphWindow.nodeNameSpaces.Any(x => nameSpace.Contains(x));
            if (isContain)
                return true;

            string graphNamespace = baseGraphWindow.GetType().Namespace;
            if (nameSpace.Contains(graphNamespace))
                return true;

            return false;
        }

        #endregion

        //通过类名找到脚本
        static MonoScript FindScriptFromClassName(string className)
        {
            var scriptGUIDs = AssetDatabase.FindAssets($"t:script {className}");

            if (scriptGUIDs.Length == 0)
                return null;

            foreach (var scriptGUID in scriptGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(scriptGUID);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);

                if (script != null && String.Equals(className, Path.GetFileNameWithoutExtension(assetPath), StringComparison.OrdinalIgnoreCase))
                    return script;
            }

            return null;
        }
    }
}
