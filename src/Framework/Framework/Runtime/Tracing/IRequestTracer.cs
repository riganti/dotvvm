using System;
using System.IO;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Runtime.Tracing
{
    public interface IRequestTracer
    {
        /// <summary> Called multiple times per request at different phases. See <see cref="RequestTracingConstants" /> for a list of possible values. Applications and other libraries may define additional events. </summary>
        Task TraceEvent(string eventName, IDotvvmRequestContext context);
    
        /// <summary> Called after the viewmodel is serialized. The <paramref name="viewModelBuffer" /> initializes a stream which can read the serialized ViewModel. </summary>
        /// <param name="viewModelSize">The size of the serialized ViewModel in bytes (uncompressed).</param>
        /// <param name="viewModelBuffer">When invoked, a new stream is created allowing to read the serialized ViewModel. The factory function must be invoked synchronously, but the Stream may be stored for further use and should be disposed when not needed anymore. </param>
        void ViewModelSerialized(IDotvvmRequestContext context, int viewModelSize, Func<Stream> viewModelBuffer);

        /// <summary> Called when DotVVM is done with handling the request and it didn't throw an unhandled exception. If <see cref="DotvvmInterruptRequestExecutionException" /> has been thrown, it has been already handled and this overload is called. </summary>
        Task EndRequest(IDotvvmRequestContext context);
        
        /// <summary> Called when DotVVM is done with handling the request and it ended up throwing an exception (different than <see cref="DotvvmInterruptRequestExecutionException" />) </summary>
        Task EndRequest(IDotvvmRequestContext context, Exception exception);
    }
}
