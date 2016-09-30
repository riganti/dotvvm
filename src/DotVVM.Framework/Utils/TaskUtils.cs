using System.Reflection;
using System.Threading.Tasks;

namespace DotVVM.Framework.Utils
{
    public static class TaskUtils
    {
        public static object GetResult(Task task)
            => IsVoidTask(task) ? null : ((dynamic)task).Result;

        private static bool IsVoidTask(Task task)
        {
            var type = task.GetType();

            if (type != typeof(Task))
            {
                return type.GetProperty("Result").PropertyType.Name == "VoidTaskResult";
            }

            return true;
        }
    }
}