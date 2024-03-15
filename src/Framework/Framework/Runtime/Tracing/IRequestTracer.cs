using System;
using System.IO;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Runtime.Tracing
{
    public interface IRequestTracer
    {
        Task TraceEvent(string eventName, IDotvvmRequestContext context);

        void ViewModelSerialized(IDotvvmRequestContext context, int viewModelSize, Lazy<Stream> viewModelBuffer);

        Task EndRequest(IDotvvmRequestContext context);

        Task EndRequest(IDotvvmRequestContext context, Exception exception);
    }
}
