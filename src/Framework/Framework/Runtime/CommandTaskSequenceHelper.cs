using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime
{
    public static class CommandTaskSequenceHelper
    {
        // do not change signatures of these methods - they are used by binding compiler to support async methods
        public static async Task JoinTasks(Task first, Func<Task> second)
        {
            await first;
            await second();
        }

        public static async Task<TResult> JoinTasks<TResult>(Task first, Func<Task<TResult>> second)
        {
            await first;
            return await second();
        }
    }
}
