using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XPToolchains.Help
{
    /// <summary>
    /// 输入框
    /// </summary>
    public class EDTypeField
    {
        public static void CreateLableField(string name, string value, float width, float height, GUIStyle style = null)
        {
            if (style != null)
            {
                EditorGUILayout.LabelField(name, value, style, GUILayout.Width(width), GUILayout.Height(height));
            }
            else
            {
                EditorGUILayout.LabelField(name, value, GUILayout.Width(width), GUILayout.Height(height));
            }

        }

        public static void CreateTypeField(string name, ref object value, Type valueType, float width, float height, GUIStyle style = null)
        {
            HandleTypeField(name, ref value, valueType, width, height, style);
        }

        private static void HandleTypeField(string name, ref object value, Type valueType, float width, float height, GUIStyle style = null)
        {
            if (valueType == typeof(int))
            {
                if (style != null)
                {
                    value = EditorGUILayout.IntField(name, (int)value, style, GUILayout.Width(width), GUILayout.Height(height));
                }
                else
                {
                    value = EditorGUILayout.IntField(name, (int)value, GUILayout.Width(width), GUILayout.Height(height));
                }
            }
            else if (valueType == typeof(float))
            {
                if (style != null)
                {
                    value = EditorGUILayout.FloatField(name, (float)value, style, GUILayout.Width(width), GUILayout.Height(height));
                }
                else
                {
                    value = EditorGUILayout.FloatField(name, (float)value, GUILayout.Width(width), GUILayout.Height(height));
                }
            }
            else if (valueType == typeof(double))
            {
                if (style != null)
                {
                    value = EditorGUILayout.DoubleField(name, (double)value, style, GUILayout.Width(width), GUILayout.Height(height));
                }
                else
                {
                    value = EditorGUILayout.DoubleField(name, (double)value, GUILayout.Width(width), GUILayout.Height(height));
                }
            }
            else if (valueType == typeof(bool))
            {
                if (style != null)
                {
                    value = EditorGUILayout.Toggle(name, (bool)value, style, GUILayout.Width(width), GUILayout.Height(height));
                }
                else
                {
                    value = EditorGUILayout.Toggle(name, (bool)value, GUILayout.Width(width), GUILayout.Height(height));
                }
            }
            else if (valueType.BaseType == typeof(Enum))
            {
                value = DrawEnumPopPanel(valueType, value, width, height, style);
            }
            else if (valueType == typeof(Vector2))
            {
                value = EditorGUILayout.Vector2Field(name, (Vector2)value, GUILayout.Width(width), GUILayout.Height(height));
            }
            else if (valueType == typeof(Vector2Int))
            {
                value = EditorGUILayout.Vector2IntField(name, (Vector2Int)value, GUILayout.Width(width), GUILayout.Height(height));
            }
            else if (valueType == typeof(Vector3))
            {
                value = EditorGUILayout.Vector3Field(name, (Vector3)value, GUILayout.Width(width), GUILayout.Height(height));
            }
            else if (valueType == typeof(Vector3Int))
            {
                value = EditorGUILayout.Vector3IntField(name, (Vector3Int)value, GUILayout.Width(width), GUILayout.Height(height));
            }
            else if (valueType == typeof(Vector4))
            {
                value = EditorGUILayout.Vector4Field(name, (Vector4)value, GUILayout.Width(width), GUILayout.Height(height));
            }
            else if (valueType == typeof(string))
            {
                if (style != null)
                {
                    value = EditorGUILayout.TextField(name, value.ToString(), style, GUILayout.Width(width), GUILayout.Height(height));
                }
                else
                {
                    value = EditorGUILayout.TextField(name, value.ToString(), GUILayout.Width(width), GUILayout.Height(height));
                }
            }
            else if (valueType == typeof(Color))
            {
                value = EditorGUILayout.ColorField(name, (Color)value, GUILayout.Width(width), GUILayout.Height(height));
            }
            else
            {
                CreateLableField(name, value.ToString(), width, height, style);
            }
        }

        private static object DrawEnumPopPanel(Type enumType, object value, float width, float height, GUIStyle style = null)
        {
            List<string> enumStrs = new List<string>();

            int selIndex = -1;
            foreach (int index in Enum.GetValues(enumType))
            {
                string strName = Enum.GetName(enumType, index);
                if (strName == value.ToString())
                {
                    selIndex = index;
                }
                enumStrs.Add(strName);
            }
            if (selIndex == -1)
            {
                selIndex = 1;
            }

            if (style != null)
            {
                selIndex = EditorGUILayout.Popup("枚举:", selIndex, enumStrs.ToArray(), style, GUILayout.Width(width), GUILayout.Height(height));
            }
            else
            {
                selIndex = EditorGUILayout.Popup("枚举:", selIndex, enumStrs.ToArray(), GUILayout.Width(width), GUILayout.Height(height));
            }
            value = Enum.GetName(enumType, selIndex);
            return value;
        }
    }
}