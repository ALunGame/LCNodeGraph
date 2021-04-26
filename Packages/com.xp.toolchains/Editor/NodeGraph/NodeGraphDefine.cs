using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace XPToolchains.NodeGraph
{
    public static class NodeGraphDefine
    {
        //样式表路径
        public const string USSPath = "Packages/com.xp.toolchains/Editor/NodeGraph/Elements/USS/";
        //结构表路径
        public const string UXMLPath = "Packages/com.xp.toolchains/Editor/NodeGraph/Elements/UXML/";
        //数据后缀名
        public const string GraphAssetExNam = ".nodeGraph";

        public static StyleSheet LoadUSS(string name)
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(USSPath + name + ".uss");
        }

        public static VisualTreeAsset LoadUXML(string name)
        {
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXMLPath + name + ".uxml");
        }


        #region Extension

        public static IEnumerable<Type> GetAllTypes(this AppDomain domain)
        {
            foreach (var assembly in domain.GetAssemblies())
            {
                Type[] types = { };

                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    //just ignore it ...
                }

                foreach (var type in types)
                    yield return type;
            }
        }

        #endregion
    }


}
