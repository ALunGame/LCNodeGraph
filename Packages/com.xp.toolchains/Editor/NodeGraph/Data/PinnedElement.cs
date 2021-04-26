using System;
using UnityEngine;

namespace XPToolchains.NodeGraph
{
    //在节点视图中固定的元素
    [Serializable]
    public class PinnedElement
    {
        public static readonly Vector2 defaultSize = new Vector2(150, 200);

        public Rect position = new Rect(Vector2.zero, defaultSize);
        public bool opened = true;
        public string editorTypeFullName;

        public PinnedElement(Type editorType)
        {
            this.editorTypeFullName = editorType.FullName;
        }

        public PinnedElement()
        {

        }
    }
}
