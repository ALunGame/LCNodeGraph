using System;

namespace XPToolchains.NodeGraph
{
    //创建一个垂直端口，而不是默认的水平端口
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class VerticalAttribute : Attribute
    {
    }

    //显示在抽屉中
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ShowAsDrawer : Attribute
    {
    }
}
