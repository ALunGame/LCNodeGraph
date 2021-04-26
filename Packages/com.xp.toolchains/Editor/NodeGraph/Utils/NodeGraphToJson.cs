using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using XPToolchains.Json;

namespace XPToolchains.NodeGraph
{
    public static class NodeGraphToJson
    {
        public static void GraphDictToJson(Dictionary<string, BaseGraph> graphDict, string savePath)
        {
            string jsonStr = JsonMapper.ToJson(graphDict);
            string dirPath = Path.GetDirectoryName(savePath);
            Debug.Log($"------> {dirPath}");
            File.WriteAllText(savePath, jsonStr, Encoding.UTF8);
        }

        public static void ToJson(BaseGraph baseGraph, string savePath)
        {
            string jsonStr = JsonMapper.ToJson(baseGraph);
            string dirPath = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            File.WriteAllText(savePath, jsonStr, Encoding.UTF8);
        }

        public static BaseGraph ToGraph(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return null;
            }
            string jsonStr = File.ReadAllText(filePath);
            return JsonMapper.ToObject<BaseGraph>(jsonStr);
        }

        public static Dictionary<string, BaseGraph> ToGraphDict(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return null;
            }
            string jsonStr = File.ReadAllText(filePath);
            return JsonMapper.ToObject<Dictionary<string, BaseGraph>>(jsonStr);
        }
    }
}
