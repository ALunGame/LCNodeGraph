using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace XPToolchains.Help
{
    //搜索输入框
    public class EDSearchInput
    {
        public static string CreateSearch(string value, string defaultStr, float width)
        {
            return CreateSearch(value, defaultStr, GUILayout.Width(width));
        }

        private static string CreateSearch(string value, string defaultStr, params GUILayoutOption[] options)
        {
            value = value == "" ? defaultStr : value;
            MethodInfo info = typeof(EditorGUILayout).GetMethod("ToolbarSearchField", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(string), typeof(GUILayoutOption[]) }, null);
            if (info != null)
            {
                value = (string)info.Invoke(null, new object[] { value, options });
            }
            return value;
        }
    }
}

