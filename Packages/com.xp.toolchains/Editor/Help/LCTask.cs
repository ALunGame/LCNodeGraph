using System;
using System.Threading.Tasks;
using UnityEngine;

namespace XPToolchains.Help
{
    /// <summary>
    /// 多线程辅助
    /// </summary>
    public class TaskHelp
    {
        public static async void AddTask(Action taskFunc, Action finishCall)
        {
            if (taskFunc == null || finishCall == null)
                return;
            try
            {
                Task runTask;
                runTask = Task.Run(taskFunc);
                await runTask;
                finishCall?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError("AddTask 异常 Thread ID :" + e);
            }
        }

        public static async void AddTask<OutT>(Func<OutT> taskFunc, Action<OutT> finishCall)
        {
            if (taskFunc == null || finishCall == null)
                return;
            OutT info = default;
            try
            {
                Task<OutT> runTask;
                runTask = Task.Run(taskFunc);
                info = await runTask;
                finishCall?.Invoke(info);
            }
            catch (Exception e)
            {
                Debug.LogError("AddTask 异常 Thread ID :" + e);
            }
        }

        public static async void AddTaskOneParam<T1, OutT>(T1 param, Func<T1, OutT> taskFunc, Action<OutT> finishCall)
        {
            if (taskFunc == null || finishCall == null)
                return;
            OutT info = default;
            try
            {
                Task<OutT> runTask = Task.Run(() =>
                {
                    return taskFunc(param);
                });
                info = await runTask;
                finishCall?.Invoke(info);
            }
            catch (Exception e)
            {
                Debug.LogError("AddTask 异常 Thread ID :" + e);
            }
        }

        public static async void AddTaskTwoParam<T1, T2, OutT>(T1 param01, T2 param02, Func<T1, T2, OutT> taskFunc, Action<OutT> finishCall)
        {
            if (taskFunc == null || finishCall == null)
                return;
            OutT info = default;
            try
            {
                Task<OutT> runTask = Task.Run(() =>
                {
                    return taskFunc(param01, param02);
                });
                info = await runTask;
                finishCall?.Invoke(info);
            }
            catch (Exception e)
            {
                Debug.LogError("AddTask 异常 Thread ID :" + e);
            }
        }

        public static async void AddTaskThreeParam<T1, T2, T3, OutT>(T1 param01, T2 param02, T3 param03, Func<T1, T2, T3, OutT> taskFunc, Action<OutT> finishCall)
        {
            if (taskFunc == null || finishCall == null)
                return;
            OutT info = default;
            try
            {
                Task<OutT> runTask = Task.Run(() =>
                {
                    return taskFunc(param01, param02, param03);
                });
                info = await runTask;
                finishCall?.Invoke(info);
            }
            catch (Exception e)
            {
                Debug.LogError("AddTask 异常 Thread ID :" + e);
            }
        }
    }
}
