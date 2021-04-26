using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace XPToolchains.Help
{
    /// <summary>
    /// 编辑器常用方法
    /// </summary>
    public class EDTool
    {
        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="str">内容</param>
        /// <param name="path">路径</param>
        public static void WriteText(string str, string path)
        {
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                sw.WriteLine(str);
            }
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="path">路径</param>
        public static string ReadText(string path)
        {
            string str = "";
            using (StreamReader sw = new StreamReader(path, Encoding.UTF8))
            {
                string tmpStr = "";
                while ((tmpStr = sw.ReadLine()) != null)
                {
                    str += tmpStr + "\n";
                }
            }
            return str;
        }

        /// <summary>
        /// 检测路径上的是否包含文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool CheckFileInPath(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// 获取父节点到子节点路径
        /// </summary>
        /// <returns></returns>
        public static string GetPathParentToChild(Transform parent, Transform child)
        {
            string path = "";

            Transform tmpParent = child.parent;
            while (tmpParent != null && tmpParent != parent)
            {
                path = tmpParent.name + "/" + path;
                tmpParent = tmpParent.parent;
            }

            path = path + child.name;
            return path;
        }

        /// <summary>
        /// 路径双斜杠
        /// </summary>
        /// <param name="path"></param>
        public static string PathToDoubleLine(string path)
        {
            string resPath = path.Replace("\\", @"\");
            return resPath;
        }

        /// <summary>
        /// 截取指定字符键后的字符串
        /// </summary>
        public static string SplitFromStrByKeyEnd(string str_source, string str_key, bool bl_contain_key)
        {
            int i_startPosition = str_source.LastIndexOf(str_key);
            if (bl_contain_key)
            {
                return str_source.Substring(i_startPosition, str_source.Length - i_startPosition);
            }
            else
            {
                return str_source.Substring(i_startPosition + str_key.Length, str_source.Length - i_startPosition - str_key.Length);
            }
        }

        /// <summary>
        /// 截取指定字符键前的字符串
        /// </summary>
        public static string SplitFromStrByKeyStart(string str_source, string str_key, bool bl_contain_key)
        {
            int i_endPosition = str_source.LastIndexOf(str_key);
            if (bl_contain_key)
            {
                return str_source.Substring(0, i_endPosition);
            }
            else
            {
                return str_source.Substring(0, i_endPosition + str_key.Length);
            }
        }

        /// <summary>
        /// 获得场景中特定组件的所有节点
        /// </summary>
        public static List<T> GetAllObjsByTypeInScene<T>(bool onlyRoot) where T : Component
        {
            T[] Objs = (T[])Resources.FindObjectsOfTypeAll(typeof(T));

            List<T> returnObjs = new List<T>();

            foreach (T Obj in Objs)
            {
                if (onlyRoot)
                {
                    if (Obj.transform.parent != null)
                    {
                        continue;
                    }
                }

                if (Obj.hideFlags == HideFlags.NotEditable || Obj.hideFlags == HideFlags.HideAndDontSave)
                {
                    continue;
                }

                if (Application.isEditor)
                {
                    //检测资源是否存在，不存在会返回null或empty的字符串，存在会返回文件名
                    string sAssetPath = AssetDatabase.GetAssetPath(Obj.transform.root.gameObject);
                    if (!string.IsNullOrEmpty(sAssetPath))
                    {
                        continue;
                    }
                }

                returnObjs.Add(Obj);
            }

            return returnObjs;
        }

        //获取目录下的所有对象路径，except mate
        public static List<string> GetAllFilePath(string path, string exName = "", bool recursive = true)
        {
            var resultList = new List<string>();

            var dirArr = Directory.GetFiles(path, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            for (int i = 0; i < dirArr.Length; i++)
            {
                if (Path.GetExtension(dirArr[i]) != ".meta")
                {
                    if (exName != "")
                    {
                        if (Path.GetExtension(dirArr[i]) == exName)
                        {
                            resultList.Add(dirArr[i].Replace('\\', '/'));
                        }
                    }
                    else
                    {
                        resultList.Add(dirArr[i].Replace('\\', '/'));
                    }

                }
            }

            return resultList;
        }

        //获取目录下的所有对象路径，except mate
        public static List<string> GetAllFilePathIgnoreName(string path, List<string> ignoreNames, bool recursive = true)
        {
            var resultList = new List<string>();

            var dirArr = Directory.GetFiles(path, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            for (int i = 0; i < dirArr.Length; i++)
            {
                if (Path.GetExtension(dirArr[i]) != ".meta")
                {
                    string exName = Path.GetExtension(dirArr[i]);
                    if (!ignoreNames.Contains(exName))
                    {
                        resultList.Add(dirArr[i].Replace('\\', '/'));
                    }

                }
            }

            return resultList;
        }
    }
}
