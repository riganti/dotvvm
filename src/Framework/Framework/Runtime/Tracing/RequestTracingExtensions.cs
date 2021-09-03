using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Runtime.Tracing
{
    public static class RequestTracingExtensions
    {
        public static async Task TracingEvent(this IEnumerable<IRequestTracer> requestTracers, string eventName, IDotvvmRequestContext context)
        {
            foreach (var tracer in requestTracers)
            {
                await tracer.TraceEvent(eventName, context);
            }
        }

        public static async Task TracingEndRequest(this IEnumerable<IRequestTracer> requestTracers, IDotvvmRequestContext context)
        {
            foreach (var tracer in requestTracers)
            {
                await tracer.EndRequest(context);
            }
        }

        public static async Task TracingException(this IEnumerable<IRequestTracer> requestTracers, IDotvvmRequestContext context, Exception exception)
        {
            foreach (var tracer in requestTracers)
            {
                await tracer.EndRequest(context, exception);
            }
        }
    }
}
