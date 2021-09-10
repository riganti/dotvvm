using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;
using DotVVM.Tracing.MiniProfiler;

namespace DotVVM.Samples.MiniProfiler.Owin.ViewModels
{
    public class DefaultStaticCommandService
    {
        public DefaultStaticCommandService(IMiniProfilerRequestTracer tracer, IDotvvmRequestContext context)
        {
            Tracer = tracer;
            Context = context;
        }

        public IMiniProfilerRequestTracer Tracer { get; }
        public IDotvvmRequestContext Context { get; }

        [AllowStaticCommand]
        public async Task StaticCommand()
        {
            Thread.Sleep(200);

            using (var timing = Tracer.Step("Static EVENT#2 "))
            {
                Thread.Sleep(80);
                using (var timing2 = Tracer.Step("Static EVENT#2.1"))
                {
                    Thread.Sleep(100);
                }
                Thread.Sleep(80);
                using (var timing2 = Tracer.Step("Static EVENT#2.2"))
                {
                    Thread.Sleep(100);
                }
            }
            using (var timing = Tracer.Step("Static EVENT#4 "))
            {
                Thread.Sleep(80);
                using (var timing2 = Tracer.Step("Static EVENT#4.1"))
                {
                    Thread.Sleep(100);
                }
                Thread.Sleep(80);
                using (var timing2 = Tracer.Step("Static EVENT#4.2"))
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}