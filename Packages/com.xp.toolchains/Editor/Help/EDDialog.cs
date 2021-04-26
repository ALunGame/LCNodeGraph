using System;
using UnityEditor;

namespace XPToolchains.Help
{
    public class EDDialog
    {
        public static void CreateDialog(string title, string message, Action okFunc = null, Action cancleFunc = null, string ok = "确定", string cancle = "取消")
        {
            bool isTrue = EditorUtility.DisplayDialog(title, message, ok, cancle);
            if (isTrue)
            {
                if (okFunc != null)
                {
                    okFunc();
                }
            }
            else
            {
                if (cancleFunc != null)
                {
                    cancleFunc();
                }
            }
        }
    }
}