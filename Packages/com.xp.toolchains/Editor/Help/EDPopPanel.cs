using System;
using UnityEditor;
using UnityEngine;

namespace XPToolchains.Help
{
    /// <summary>
    /// 编辑器弹窗
    /// </summary>
    public class EDPopPanel : EditorWindow
    {
        public string InputStr = "";
        public Action<string> CallBack;

        private void OnGUI()
        {
            EDLayout.CreateVertical("box", position.width, position.height, () =>
            {
                InputStr = EditorGUILayout.TextField("请输入：", InputStr);

                EditorGUILayout.Space();

                EDButton.CreateBtn("确定", position.width * 0.9f, position.height * 0.5f, () =>
                {
                    if (CallBack != null && InputStr != "")
                    {
                        CallBack(InputStr);
                        Close();
                    }
                });
            });
        }

        public static void PopWindow(string strContent, Action<string> callBack)
        {
            Rect rect = new Rect(Event.current.mousePosition, new Vector2(250, 80));
            //Rect rect = new Rect(new Vector2(0,0),new Vector2(250, 80));
            EDPopPanel window = GetWindowWithRect<EDPopPanel>(rect, true, strContent);
            //window.position = rect;
            window.CallBack = callBack;
            window.Focus();
        }

        public static void PopWindow(string strContent, Vector2 pos, Action<string> callBack)
        {
            Rect rect = new Rect(pos, new Vector2(250, 80));
            EDPopPanel window = GetWindowWithRect<EDPopPanel>(rect, true, strContent);
            window.CallBack = callBack;
            window.Focus();
        }
    }
}
