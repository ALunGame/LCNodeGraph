using UnityEditor;

namespace XPToolchains.Help
{
    //打开文件选择窗口
    public class EDOpenFloder
    {
        /// <summary>
        /// 打开文件夹
        /// </summary>
        public static string OpenFolderPanel(string title)
        {
            var floder = "";
            floder = EditorUtility.OpenFolderPanel(title, floder, "");
            return floder;
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        public static string OpenFilePanelWithExtName(string title, string directory, string extension)
        {
            return EditorUtility.OpenFilePanel(title, directory, extension);

        }

        /// <summary>
        /// 打开文件
        /// </summary>
        public static string OpenFilePanelWithFilter(string title, string directory, params string[] filters)
        {
            if (filters == null)
            {
                return EditorUtility.OpenFilePanelWithFilters(title, directory, new string[] { "*" });
            }
            else
            {
                return EditorUtility.OpenFilePanelWithFilters(title, directory, filters);
            }
        }
    }
}
