#nullable enable
using System;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Runtime.Tracing
{
    public interface IRequestTracer
    {
        Task TraceEvent(string eventName, IDotvvmRequestContext context);

        Task EndRequest(IDotvvmRequestContext context);

        Task EndRequest(IDotvvmRequestContext context, Exception exception);
    }
}
