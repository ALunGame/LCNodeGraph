using System;

namespace XPToolchains.NodeGraph
{
    //节点用的属性

    //标记改类可以被菜单创建
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class NodeMenuItemAttribute : Attribute
    {
        public string menuTitle;
        public Type serializeType;
        public NodeMenuItemAttribute(string menuTitle = null,Type serializeType = null)
        {
            this.menuTitle = menuTitle;
            this.serializeType = serializeType;
        }
    }

    //自定义节点视图
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeCustomEditor : Attribute
    {
        public Type nodeType;

        public NodeCustomEditor(Type nodeType)
        {
            this.nodeType = nodeType;
        }
    }

    //输入端口
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class InputAttribute : Attribute
    {
        public string name;
        //允许多个
        public bool allowMultiple = false;

        public InputAttribute(string name = null, bool allowMultiple = false)
        {
            this.name = name;
            this.allowMultiple = allowMultiple;
        }
    }

    //输出端口
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class OutputAttribute : Attribute
    {
        public string name;
        public bool allowMultiple = true;

        public OutputAttribute(string name = null, bool allowMultiple = true)
        {
            this.name = name;
            this.allowMultiple = allowMultiple;
        }
    }
}
