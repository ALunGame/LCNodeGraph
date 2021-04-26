using System;
using System.Collections.Generic;
using System.Reflection;

namespace XPToolchains.Json.Extensions
{

    /// <summary>
    /// 拓展方法
    /// </summary>
    public static class Extensions
    {

        public static void WriteProperty(this JsonWriter w, string name, long value)
        {
            w.WritePropertyName(name);
            w.Write(value);
        }

        public static void WriteProperty(this JsonWriter w, string name, string value)
        {
            w.WritePropertyName(name);
            w.Write(value);
        }

        public static void WriteProperty(this JsonWriter w, string name, bool value)
        {
            w.WritePropertyName(name);
            w.Write(value);
        }

        public static void WriteProperty(this JsonWriter w, string name, double value)
        {
            w.WritePropertyName(name);
            w.Write(value);
        }

        private static Dictionary<string, Type> TypeCache = new Dictionary<string, Type>();
        public static Type GetTypeByFullName(string fullName)
        {
            if (TypeCache.ContainsKey(fullName))
            {
                return TypeCache[fullName];
            }
            Type type = null;
            Assembly[] assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
            int assemblyArrayLength = assemblyArray.Length;
            for (int i = 0; i < assemblyArrayLength; ++i)
            {
                type = assemblyArray[i].GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            for (int i = 0; (i < assemblyArrayLength); ++i)
            {
                Type[] typeArray = assemblyArray[i].GetTypes();
                int typeArrayLength = typeArray.Length;
                for (int j = 0; j < typeArrayLength; ++j)
                {
                    if (typeArray[j].Name.Equals(fullName))
                    {
                        return typeArray[j];
                    }
                }
            }
            return type;

        }
    }
}
