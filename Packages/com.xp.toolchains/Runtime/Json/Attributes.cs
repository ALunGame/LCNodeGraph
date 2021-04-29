using System;

namespace XPToolchains.Json
{
    /// <summary>
    /// 跳过序列化的标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class JsonIgnore : Attribute
    {

    }
}
