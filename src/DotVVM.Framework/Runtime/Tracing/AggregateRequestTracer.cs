#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Runtime.Tracing
{
    public class AggregateRequestTracer
    {
        private readonly IRequestTracer[] tracers;

        public AggregateRequestTracer(IServiceProvider services)
        {
            this.tracers = services.GetServices<IRequestTracer>().ToArray();
        }

        public Task TraceEvent(string eventName, IDotvvmRequestContext context) =>
            tracers.TracingEvent(eventName, context);

        public Task EndRequest(IDotvvmRequestContext context) =>
            tracers.TracingEndRequest(context);

        public Task EndRequest(IDotvvmRequestContext context, Exception exception) =>
            tracers.TracingException(context, exception);
    }
}
