using System;
using UnityEngine;

namespace XPToolchains.Help
{
    public class EDButton
    {
        public static void CreateBtn(string btnName, float width, float height, Action callBack)
        {
            if (GUILayout.Button(new GUIContent(btnName), GUI.skin.button, GUILayout.Width(width), GUILayout.Height(height)))
            {
                callBack?.Invoke();
            }
        }

        public static void CreateBtn(object btnName, float width, float height, Action callBack)
        {
            if (GUILayout.Button(new GUIContent(btnName.ToString()), GUI.skin.button, GUILayout.Width(width), GUILayout.Height(height)))
            {
                callBack?.Invoke();
            }
        }
    }
}
