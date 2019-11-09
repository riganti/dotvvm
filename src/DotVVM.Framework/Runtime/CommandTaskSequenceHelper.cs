#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime
{
    public static class CommandTaskSequenceHelper
    {

        // do not change these methods - they are used by binding compiler to support async methods

        public static Task JoinTasks(Task first, Func<Task> second)
        {
            return first.ContinueWith(t => second()).Unwrap();
        }

        public static Task<TResult> JoinTasks<TResult>(Task first, Func<Task<TResult>> second)
        {
            return first.ContinueWith(t => second()).Unwrap();
        }

        public static Task WrapAsTask(Action action)
        {
            action();
            return TaskUtils.GetCompletedTask();
        }

        public static Task<T> WrapAsTask<T>(Func<T> action)
        {
            return Task.FromResult(action());
        }
    }
}
