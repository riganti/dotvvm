using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

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

        public static void TracingSerialized(this IEnumerable<IRequestTracer> requestTracers, IDotvvmRequestContext context, int viewModelSize, MemoryStream stream)
        {
            bool calledSynchronously = false;
            foreach (var tracer in requestTracers)
            {
                tracer.ViewModelSerialized(context, viewModelSize, () => {
                    if (!calledSynchronously) // make sure we can optimize this later to use buffers from ArrayPool
                        throw new InvalidOperationException("The stream factory function is being invoked too late.");
                    return stream.CloneReadOnly();
                });
            }
            calledSynchronously = true;
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
