#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using System;
using System.Collections.Generic;

public static partial class Extension
{
    public static bool TryGet<T>(this T[] _array, int _index, out T _element)
    {
        _element = default;
        if (_array.Length > _index)
        {
            _element = _array[_index];
            return true;
        }
        return false;
    }

    static Random Random { get; set; } = new Random();

    /// <summary> 快速排序(第二个参数是中间值) </summary>
    public static void QuickSort<T>(this List<T> _original, Func<T, T, bool> _func)
    {
        if (_original.Count == 0)
            return;
        if (_original.Count == 1)
            return;

        Random.Next(0, _original.Count);
        // 抽取一个数据作为中间值
        int index = UnityEngine.Random.Range(0, _original.Count);
        T rN = _original[index];

        // 声明小于中间值的列表
        List<T> left = new List<T>(Math.Max(4, _original.Count / 2));
        // 声明大于中间值的列表
        List<T> right = new List<T>(Math.Max(4, _original.Count / 2));
        // 遍历数组，与中间值比较，小于中间值的放在left，大于中间值的放在right
        for (int i = 0; i < _original.Count; i++)
        {
            if (i == index) continue;

            if (_func(_original[i], rN))
                left.Add(_original[i]);
            else
                right.Add(_original[i]);
        }

        _original.Clear();

        // 如果左列表元素个数不为0，就把左列表也排序
        if (left.Count != 0)
        {
            left.QuickSort(_func);
            _original.AddRange(left);
        }
        _original.Add(rN);
        // 如果右列表元素个数不为0，就把右列表也排序
        if (right.Count != 0)
        {
            right.QuickSort(_func);
            _original.AddRange(right);
        }
        return;
    }

}
