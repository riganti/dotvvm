using System;
using System.IO;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Tracing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class RequestTracingExtensionsTests
    {
        [TestMethod]
        public void TracingSerialized_StreamFactoryCanBeCalledSynchronously()
        {
            var tracer = new SynchronousStreamReaderTracer();
            using var stream = new MemoryStream([1, 2, 3]);

            new[] { tracer }.TracingSerialized(context: null!, viewModelSize: 3, stream);

            Assert.IsTrue(tracer.Called);
        }

        [TestMethod]
        public void TracingSerialized_StreamFactoryThrowsWhenInvokedTooLate()
        {
            var tracer = new DeferredInvocationTracer();
            using var stream = new MemoryStream([1, 2, 3]);

            new[] { tracer }.TracingSerialized(context: null!, viewModelSize: 3, stream);

            var exception = Assert.ThrowsException<InvalidOperationException>(() => tracer.StreamFactory!());
            StringAssert.Contains(exception.Message, "invoked too late");
        }

        private class SynchronousStreamReaderTracer : IRequestTracer
        {
            public bool Called { get; private set; }

            public void ViewModelSerialized(IDotvvmRequestContext context, int viewModelSize, Func<Stream> viewModelBuffer)
            {
                using var _ = viewModelBuffer();
                Called = true;
            }

            public Task EndRequest(IDotvvmRequestContext context) => Task.CompletedTask;

            public Task EndRequest(IDotvvmRequestContext context, Exception exception) => Task.CompletedTask;

            public Task TraceEvent(string eventName, IDotvvmRequestContext context) => Task.CompletedTask;
        }

        private class DeferredInvocationTracer : IRequestTracer
        {
            public Func<Stream>? StreamFactory { get; private set; }

            public void ViewModelSerialized(IDotvvmRequestContext context, int viewModelSize, Func<Stream> viewModelBuffer)
            {
                StreamFactory = viewModelBuffer;
            }

            public Task EndRequest(IDotvvmRequestContext context) => Task.CompletedTask;

            public Task EndRequest(IDotvvmRequestContext context, Exception exception) => Task.CompletedTask;

            public Task TraceEvent(string eventName, IDotvvmRequestContext context) => Task.CompletedTask;
        }
    }
}
