using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XPToolchains.Help
{
    /// <summary>
    /// 下拉框
    /// </summary>
    public class EDDropdown
    {
        public static void CreateDropdown(string selItem, List<string> itemList, Action<string> selCallBack, float width = 100)
        {
            if (EditorGUILayout.DropdownButton(new GUIContent(selItem), FocusType.Keyboard, GUILayout.Width(width)))
            {
                GenericMenu menu = new GenericMenu();
                foreach (string item in itemList)
                {
                    AddMenuItem(menu, item, selItem, selCallBack);
                }
                menu.ShowAsContext();
            }
        }

        private static void AddMenuItem(GenericMenu menu, string item, string selItem, Action<string> selCallBack)
        {
            menu.AddItem(new GUIContent(item), selItem.Equals(item), (x) =>
            {

                selItem = x.ToString();
                if (selCallBack != null)
                {
                    selCallBack(x.ToString());
                }

            }, item);
        }

        public static void CreateDropdown(string selItem, List<string> itemList, Action<int> selCallBack, float width = 100)
        {
            if (EditorGUILayout.DropdownButton(new GUIContent(selItem), FocusType.Keyboard, GUILayout.Width(width)))
            {
                GenericMenu menu = new GenericMenu();

                for (int i = 0; i < itemList.Count; i++)
                {
                    AddMenuItem(menu, itemList[i], selItem, i, selCallBack);
                }
                menu.ShowAsContext();
            }
        }

        private static void AddMenuItem(GenericMenu menu, string item, string selItem, int index, Action<int> selCallBack)
        {
            menu.AddItem(new GUIContent(item), selItem.Equals(item), (x) =>
            {

                selItem = x.ToString();
                if (selCallBack != null)
                {
                    selCallBack(index);
                }

            }, item);
        }
    }
}
