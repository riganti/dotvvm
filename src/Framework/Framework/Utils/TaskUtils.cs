using System;
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
        {
            var type = task.GetType();
            if (type == typeof(Task))
            {
                return null;
            }

            var resultProperty = type.GetProperty("Result");
            if (resultProperty is null)
                return null;

            var taskResultPropertyName = resultProperty.PropertyType.Name;
            if (taskResultPropertyName == "VoidTaskResult" || taskResultPropertyName == "VoidResult")
            {
                return null;
            }


            // throw exception without the TargetInvocationException wrapper
            if (task.Status != TaskStatus.RanToCompletion)
                task.Wait();

            return resultProperty.GetValue(task);
        }
    }
}
