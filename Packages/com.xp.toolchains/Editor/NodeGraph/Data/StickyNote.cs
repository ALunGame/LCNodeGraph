using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XPToolchains.NodeGraph
{
    /// <summary>
    /// 注释
    /// </summary>
    public class StickyNote
    {
        public Rect position;
        public string title = "标题";
        public string content = "内容";

        public StickyNote()
        {

        }
        public StickyNote(string title, Vector2 position)
        {
            this.title = title;
            this.position = new Rect(position.x, position.y, 200, 300);
        }
    }
}
