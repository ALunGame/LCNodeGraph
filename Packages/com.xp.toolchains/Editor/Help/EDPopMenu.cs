using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XPToolchains.Help
{
    /// <summary>
    /// 编辑器弹出菜单
    /// </summary>
    public class EDPopMenu
    {
        public static void CreatePopMenu(List<string> itemList, Action<string> selCallBack)
        {
            GenericMenu menu = new GenericMenu();
            foreach (string item in itemList)
            {
                AddMenuItem(menu, item, selCallBack);
            }
            menu.ShowAsContext();
        }

        private static void AddMenuItem(GenericMenu menu, string item, Action<string> selCallBack)
        {
            menu.AddItem(new GUIContent(item), false, (x) =>
            {
                selCallBack?.Invoke(x.ToString());
            }, item);
        }

        private static void AddMenuItem(GenericMenu menu, string item, int index, Action<int> selCallBack)
        {
            menu.AddItem(new GUIContent(item), false, (x) =>
            {
                if (selCallBack != null)
                {
                    selCallBack(index);
                }

            }, item);
        }

        public static void CreatePopMenu(List<string> itemList, Action<int> selCallBack)
        {
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < itemList.Count; i++)
            {
                AddMenuItem(menu, itemList[i], i, selCallBack);
            }
            menu.ShowAsContext();
        }

        private static void AddMenuItem(GenericMenu menu, string item, int index, Action<int, string> selCallBack)
        {
            menu.AddItem(new GUIContent(item), false, (x) =>
            {
                if (selCallBack != null)
                {
                    selCallBack(index, item);
                }

            }, item);
        }

        public static void CreatePopMenu(List<string> itemList, Action<int, string> selCallBack)
        {
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < itemList.Count; i++)
            {
                AddMenuItem(menu, itemList[i], i, selCallBack);
            }
            menu.ShowAsContext();
        }
    }
}
