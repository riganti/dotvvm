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

        /// <summary> Converts Task, Task{T} or any object to Task{object}. Other types of awaitable objects are not supported, and an exception will be thrown when used. </summary>
        public async static Task<object?> ToObjectTask(object? taskOrSomething)
        {
            if (taskOrSomething is Task commandTask)
            {
                await commandTask;
                return TaskUtils.GetResult(commandTask);
            }

            var resultType = taskOrSomething?.GetType();
            var possibleResultAwaiter = resultType?.GetMethod(nameof(Task.GetAwaiter), new Type[] { });

            if (resultType != null && possibleResultAwaiter != null)
            {
                throw new NotSupportedException($"The command uses unsupported awaitable type {resultType.FullName}, please use System.Task instead.");
            }
            return taskOrSomething;
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
