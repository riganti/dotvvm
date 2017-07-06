using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Runtime.Tracing
{
    public interface IRequestTracingReporter
    {
        Task StartTraceEvents();

        Task TraceEvents(IDotvvmRequestContext context, Dictionary<string, object> traceData);

        Task TraceException(IDotvvmRequestContext context, Exception e);
    }
}
