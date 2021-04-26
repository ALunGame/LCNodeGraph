using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XPToolchains.Help
{
    public class EDScene
    {
        public static List<T> GetSceneObjects<T>() where T : Component
        {
            Object[] gos = Resources.FindObjectsOfTypeAll(typeof(T));

            List<T> resComs = new List<T>();
            for (int i = 0; i < gos.Length; i++)
            {
                if (gos[i].hideFlags == HideFlags.None)
                {
                    //Debug.LogWarning(gos[i].GetType().FullName)
                    resComs.Add(((T)gos[i]).GetComponent<T>());
                }
            }

            return resComs;
        }
    }
}
