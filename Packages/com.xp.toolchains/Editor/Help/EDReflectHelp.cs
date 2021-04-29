using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace XPToolchains.Help
{
    /// <summary>
    /// 编辑器反射辅助类
    /// </summary>
    public class EDReflectHelp
    {
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

        public static List<Type> GetAllClassByClass<T>(string assemblyPath = "")
        {
            string asPath = assemblyPath;
            if (asPath == "")
            {
                asPath = Path.GetFullPath(@".\Library\ScriptAssemblies\Assembly-CSharp.dll");
            }
            List<Type> resTypes = new List<Type>();

            //获得程序集
            Assembly ass = Assembly.LoadFile(@asPath);
            Type[] typs = ass.GetTypes();

            //Debug.LogError("目标类>>>>>>>" + typeof(T).FullName);
            foreach (Type item in typs)
            {

                //获得子类
                if (item.BaseType != null && item.BaseType.FullName == typeof(T).FullName)
                {
                    resTypes.Add(item);
                }
            }
            return resTypes;
        }

        public static List<Type> GetAllClassByClass(string classFullName, string assemblyPath = "")
        {
            string asPath = assemblyPath;
            if (asPath == "")
            {
                asPath = Path.GetFullPath(@".\Library\ScriptAssemblies\Assembly-CSharp.dll");
            }
            List<Type> resTypes = new List<Type>();

            //获得程序集
            Assembly ass = Assembly.LoadFile(@asPath);
            Type[] typs = ass.GetTypes();

            //Debug.LogError("目标类>>>>>>>" + classFullName);
            foreach (Type item in typs)
            {
                //获得子类
                if (item.BaseType != null && item.BaseType.FullName == classFullName)
                {
                    resTypes.Add(item);
                }
            }
            return resTypes;
        }

        public static List<Type> GetAllInterfaceByInterface<T>(string assemblyPath = "")
        {
            string asPath = assemblyPath;
            if (asPath == "")
            {
                asPath = Path.GetFullPath(@".\Library\ScriptAssemblies\Assembly-CSharp.dll");
            }
            List<Type> resTypes = new List<Type>();

            //获得程序集
            Assembly ass = Assembly.LoadFile(@asPath);
            Type[] typs = ass.GetTypes();

            //Debug.LogError("目标类>>>>>>>" + typeof(T).FullName);
            foreach (Type item in typs)
            {
                //获得接口
                if (item.GetInterface(typeof(T).FullName) != null && item.IsAbstract)
                {
                    resTypes.Add(item);
                }
            }
            return resTypes;
        }

        public static List<Type> GetAllInterfaceByInterface(string interfaceFullName, string assemblyPath = "")
        {
            string asPath = assemblyPath;
            if (asPath == "")
            {
                asPath = Path.GetFullPath(@".\Library\ScriptAssemblies\Assembly-CSharp.dll");
            }
            List<Type> resTypes = new List<Type>();

            //获得程序集
            Assembly ass = Assembly.LoadFile(@asPath);
            Type[] typs = ass.GetTypes();

            //Debug.LogError("目标类>>>>>>>" + typeof(T).FullName);
            foreach (Type item in typs)
            {
                //获得接口
                if (item.GetInterface(interfaceFullName) != null && item.IsAbstract)
                {
                    resTypes.Add(item);
                }
            }
            return resTypes;
        }

        public static Type GetTypeByFullName(string fullName, string assemblyPath = "")
        {
            string asPath = assemblyPath;
            if (asPath == "")
            {
                asPath = Path.GetFullPath(@".\Library\ScriptAssemblies\Assembly-CSharp.dll");
            }
            List<Type> resTypes = new List<Type>();

            //获得程序集
            Assembly ass = Assembly.LoadFile(@asPath);
            Type typ = ass.GetType(fullName);
            return typ;
        }
    }
}
