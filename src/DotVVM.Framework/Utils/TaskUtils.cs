using System.Threading.Tasks;

namespace DotVVM.Framework.Utils
{
    public static class TaskUtils
    {
        public static object GetResult(Task task) => task.GetType() == typeof(Task) ? null : ((dynamic)task).Result;
    }
}