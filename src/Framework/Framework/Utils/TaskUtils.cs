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
