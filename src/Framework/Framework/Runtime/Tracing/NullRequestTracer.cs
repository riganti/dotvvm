using System;
using System.IO;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime.Tracing
{

    public class NullRequestTracer : IRequestTracer
    {
        private NullRequestTracer() { }
        public Task TraceEvent(string eventName, IDotvvmRequestContext context)
        {
            return TaskUtils.GetCompletedTask();
        }

        public Task EndRequest(IDotvvmRequestContext context)
        {
            return TaskUtils.GetCompletedTask();
        }

        public Task EndRequest(IDotvvmRequestContext context, Exception exception)
        {
            return TaskUtils.GetCompletedTask();
        }

        public void ViewModelSerialized(IDotvvmRequestContext context, int viewModelSize, Func<Stream> viewModelBuffer)
        {
        }

        public readonly static NullRequestTracer Instance = new NullRequestTracer();
    }

}
