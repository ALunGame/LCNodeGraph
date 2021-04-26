using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace XPToolchains.NodeGraph
{
    /// <summary>
    /// 版本回退辅助
    /// </summary>
    public static class RollbackHelper
    {
        public static Dictionary<string, BaseGraph> GetGraphDict(List<string> graphFilePathList)
        {
            Dictionary<string, BaseGraph> graphDict = new Dictionary<string, BaseGraph>();
            for (int i = 0; i < graphFilePathList.Count; i++)
            {
                BaseGraph graph = NodeGraphToJson.ToGraph(graphFilePathList[i]);
                graphDict.Add(graphFilePathList[i], graph);
            }
            return graphDict;
        }

        public static void SaveRollbackVer(string savePath, List<string> graphFilePathList)
        {
            if (graphFilePathList == null || graphFilePathList.Count <= 0)
            {
                return;
            }
            Dictionary<string, BaseGraph> graphDict = GetGraphDict(graphFilePathList);
            DateTime date = DateTime.Now;
            string fileName = string.Format("{0:yyyy-MM-dd HH-mm-ss}", date);
            NodeGraphToJson.GraphDictToJson(graphDict, savePath + "/" + fileName + NodeGraphDefine.GraphAssetExNam);
        }

        public static List<string> GetRollbackVerPathList(string savePath)
        {
            List<string> resPathList = new List<string>();
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            resPathList.AddRange(Directory.GetFiles(savePath, "*" + NodeGraphDefine.GraphAssetExNam, SearchOption.AllDirectories));
            return resPathList;
        }

        public static void DelTopBackVer(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }
            File.Delete(filePath);
            AssetDatabase.Refresh();
        }
        public static Dictionary<string, BaseGraph> GetRollbackVer(string filePath)
        {
            return NodeGraphToJson.ToGraphDict(filePath);
        }
    }
}
