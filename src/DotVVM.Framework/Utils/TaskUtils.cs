using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Utils
{
    public static class TaskUtils
    {
        private static Task completedTask;
        public static Task GetCompletedTask() => completedTask ?? (completedTask = Task.WhenAll());

        public static object GetResult(Task task) => task.GetType() == typeof(Task) ? null : ((dynamic)task).Result;

    }
}
