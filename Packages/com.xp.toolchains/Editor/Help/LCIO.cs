using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XPToolchains.Help
{
    /// <summary>
    /// 文件读写
    /// </summary>
    public class LCIO
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
            if (!File.Exists(path))
            {
                return "";
            }
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
        /// 删除文件
        /// </summary>
        public static void DelFile(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }
            File.Delete(path);
        }

        /// <summary>
        /// 删除目录下所有文件
        /// </summary>
        public static void DelDirectoryAllFile(string path, string exName = "", bool recursive = true)
        {
            List<string> allFile = GetAllFilePath(path, exName, recursive);
            for (int i = 0; i < allFile.Count; i++)
            {
                DelFile(allFile[i]);
            }
        }


        /// <summary>
        /// 获取目录下的所有对象路径 忽略meta文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="exName">扩展名</param>
        /// <param name="recursive">子目录搜索</param>
        /// <returns></returns>
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

        /// <summary>
        /// 获取目录下的所有对象路径 忽略meta文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="ignoreExNames">忽略的扩展名</param>
        /// <param name="recursive">子目录搜索</param>
        /// <returns></returns>
        public static List<string> GetAllFilePathIgnoreName(string path, List<string> ignoreExNames, bool recursive = true)
        {
            var resultList = new List<string>();

            var dirArr = Directory.GetFiles(path, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            for (int i = 0; i < dirArr.Length; i++)
            {
                if (Path.GetExtension(dirArr[i]) != ".meta")
                {
                    string exName = Path.GetExtension(dirArr[i]);
                    if (!ignoreExNames.Contains(exName))
                    {
                        resultList.Add(dirArr[i].Replace('\\', '/'));
                    }

                }
            }

            return resultList;
        }
    }
}
