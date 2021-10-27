﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DotVVM.Framework.Utils
{
    public static class TaskUtils
    {
        public static Task GetCompletedTask()
        {
            return Task.CompletedTask;
        }

        public static object? GetResult(Task task)
            => IsVoidTask(task) ? null : ((dynamic)task).Result;

        private static bool IsVoidTask(Task task)
        {
            var type = task.GetType();

            var resultProperty = type.GetProperty("Result");
            if (type != typeof(Task) && resultProperty != null)
            {
                var taskResultPropertyName = resultProperty.PropertyType.Name;
                return taskResultPropertyName == "VoidTaskResult" || taskResultPropertyName == "VoidResult";
            }

            return true;
        }


    }
}
