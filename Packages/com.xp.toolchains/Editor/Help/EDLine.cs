using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace XPToolchains.Help
{
    /// <summary>
    /// 编辑器画线
    /// </summary>
    public class EDLine
    {
        /// <summary>
        /// 画贝赛尔曲线
        /// </summary>
        public static void CreateBezierLine(Vector3 startPosition, Vector3 endPosition, float width, Color color = default(Color), Texture2D texture = null)
        {
            Handles.DrawBezier(startPosition, endPosition, startPosition - Vector3.left * 50f, endPosition + Vector3.left * 50f, color, texture, width);
        }

        /// <summary>
        /// 绘制边框
        /// </summary>
        private static Type EditorGUI;
        private static object EditorGUIObj;
        public static void DrawOutline(Rect rect, float size, Color color)
        {
            if (EditorGUI == null)
            {
                Assembly asm = Assembly.Load("UnityEditor");
                EditorGUI = asm.GetType("UnityEditor.EditorGUI");
                EditorGUIObj = Activator.CreateInstance(EditorGUI);
            }
            MethodInfo oMethod = EditorGUI.GetMethod("DrawOutline", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            //MethodInfo oMethod = EditorGUI.GetMethod("DrawOutline", BindingFlags.Static);
            oMethod.Invoke(EditorGUIObj, new Object[] { rect, size, color });
        }
    }
}
