using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace XPToolchains
{
    /// <summary>
    /// 开发中快捷键
    /// </summary>
    public class DevelopShorcutKey
    {
        #region Unity启动停止

        [MenuItem("Edit/快捷键/运行Unity _F5")]
        private static void Run()
        {
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Edit/快捷键/运行Unity _F5", true)]
        private static bool CanRun()
        {
            return !EditorApplication.isPlaying;
        }

        [MenuItem("Edit/快捷键/关闭Unity #_F5")]
        private static void Stop()
        {
            EditorApplication.isPlaying = false;
        }

        [MenuItem("Edit/快捷键/关闭Unity #_F5", true)]
        private static bool CanStop()
        {
            return EditorApplication.isPlaying;
        }

        #endregion

        #region 节点序号

        private static Regex m_regex = new Regex(@"(.*)(\([0-9]*\))");
        [MenuItem("Edit/快捷键/清除节点序号 &r")]
        private static void Remove()
        {
            var list = Selection.gameObjects
                    .Where(c => m_regex.IsMatch(c.name))
                    .ToArray()
                ;

            if (list.Length == 0) return;

            foreach (var n in list)
            {
                Undo.RecordObject(n, "Remove Duplicated Name");
                n.name = m_regex.Replace(n.name, @"$1");
                n.name = n.name.Replace(" ", "");
            }
        }

        [MenuItem("Edit/快捷键/清除节点序号 &r", true)]
        private static bool CanRemove()
        {
            var gameObjects = Selection.gameObjects;
            return gameObjects != null && 0 < gameObjects.Length;
        }

        [MenuItem("Edit/快捷键/创建不带节点序号的节点 &d")]
        private static void Duplicate()
        {
            var list = new List<int>();

            foreach (var n in Selection.gameObjects)
            {
                var clone = Object.Instantiate(n, n.transform.parent);
                clone.name = n.name;
                list.Add(clone.GetInstanceID());
                Undo.RegisterCreatedObjectUndo(clone, "Duplicate Without Serial Number");
            }

            Selection.instanceIDs = list.ToArray();
            list.Clear();
        }

        [MenuItem("Edit/快捷键/创建不带节点序号的节点 &d", true)]
        private static bool CanDuplicate()
        {
            var gameObjects = Selection.gameObjects;
            return gameObjects != null && 0 < gameObjects.Length;
        }

        #endregion

        #region Help

        [MenuItem("GameObject/UI/UIKit/_生成Coms(C) #&C")]
        static void CreateComsCode()
        {
            CreateAutoCode(new List<GameObject>(Selection.gameObjects), "RectTransform");
        }

        [MenuItem("GameObject/UI/UIKit/_复制节点路径(P) #&P")]
        static void CreateUIPath()
        {
            GameObject rootGo = GetUIPanelGo(Selection.activeGameObject);
            string strPath = GetPathParentToChild(rootGo.transform, Selection.activeGameObject.transform);
            GUIUtility.systemCopyBuffer = strPath;
            Debug.LogError("路径复制成功：" + strPath);
        }

        [MenuItem("GameObject/UI/UIKit/_复制节点相对路径(Q) #&Q")]
        static void CreatedDoubleUIPath()
        {
            GameObject[] objs = Selection.gameObjects;
            if (objs == null || objs.Length != 2)
            {
                return;
            }

            string strPath = GetPathParentToChild(objs[0].transform, objs[1].transform);
            GUIUtility.systemCopyBuffer = strPath;
            Debug.LogError("路径复制成功：" + strPath);
        }

        [MenuItem("GameObject/UI/UIKit/_创建空节点(E) #&E")]
        static void CreateEmptyGo()
        {
            GameObject go = new GameObject("Root");
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.localScale = Vector3.one;
            go.transform.SetParent(Selection.activeGameObject.transform);
            rect.localScale = Vector3.one;
        }

        [MenuItem("GameObject/UI/UIKit/_创建按钮(B) #&B")]
        static void CreateBtnGo()
        {
            GameObject go = new GameObject("Btn");
            RectTransform rect = go.AddComponent<RectTransform>();
            Image img = go.AddComponent<Image>();
            img.raycastTarget = true;
            rect.localScale = Vector3.one;
            go.transform.SetParent(Selection.activeGameObject.transform);

            GameObject goImg = new GameObject("Img");
            RectTransform rectImg = goImg.AddComponent<RectTransform>();
            Image img02 = goImg.AddComponent<Image>();
            img02.raycastTarget = false;
            rectImg.localScale = Vector3.one;
            goImg.transform.SetParent(go.transform);

            GameObject txtGo = new GameObject("Txt");
            RectTransform rectTxt = txtGo.AddComponent<RectTransform>();
            Text txt = txtGo.AddComponent<Text>();
            txt.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/GameAssets/BundleAssets/Common/Fonts/FZKTJW.ttf");
            txt.raycastTarget = false;
            rectTxt.localScale = Vector3.one;
            txtGo.transform.SetParent(go.transform);

            rect.localScale = Vector3.one;
        }

        //检测是不是根节点
        private static bool CheckIsRootGo(GameObject go)
        {
            //安全保护
            if (go == null || go.transform.parent == null)
            {
                return true;
            }
            if (go.name.Contains("Panel"))
            {
                return true;
            }
            if (go.name.Contains("PartialView"))
            {
                return true;
            }

            if (go.GetComponent<Canvas>() != null)
            {
                return true;
            }

            return false;
        }

        static GameObject GetUIPanelGo(GameObject gameObject)
        {
            GameObject uiPanel = null;
            while (true)
            {
                if (CheckIsRootGo(gameObject))
                {
                    uiPanel = gameObject;
                    break;
                }
                else
                {
                    gameObject = gameObject.transform.parent.gameObject;
                }
            }
            return uiPanel;
        }

        /// <summary>
        /// 获取父节点到子节点路径
        /// </summary>
        /// <returns></returns>
        public static string GetPathParentToChild(Transform parent, Transform child)
        {
            string path = "";
            if (parent == null || child == null)
            {
                return path;
            }

            Transform tmpParent = child.parent;
            while (tmpParent != null && tmpParent != parent)
            {
                path = tmpParent.name + "/" + path;
                tmpParent = tmpParent.parent;
            }

            path = path + child.name;
            return path;
        }

        static void CreateAutoCode(List<GameObject> chidNodes, string comType)
        {
            if (chidNodes == null || chidNodes.Count <= 0)
            {
                return;
            }
            GameObject uiPanel = GetUIPanelGo(chidNodes[0]);

            string cpyStr = "";

            for (int i = 0; i < chidNodes.Count; i++)
            {
                string path = GetPathParentToChild(uiPanel.transform, chidNodes[i].transform);
                cpyStr += CreateUIComLuaCode(chidNodes[i], path, comType) + "\n";
            }

            GUIUtility.systemCopyBuffer = cpyStr;
            Debug.LogError("代码复制成功：" + cpyStr);
        }
        private const string ComTabValStr = "\t{0} = {{Path = {1}, Type = {2}}},";
        static string CreateUIComLuaCode(GameObject childNode, string path, string comType)
        {
            return string.Format(ComTabValStr, childNode.name, string.Format("\"{0}\"", path), string.Format("\"{0}\"", comType));
        }

        #endregion
    }
}
