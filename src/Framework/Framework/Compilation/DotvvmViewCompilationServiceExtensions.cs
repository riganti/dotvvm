using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Tracing;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Compilation
{
    public static class DotvvmViewCompilationServiceExtensions
    {
        public static Task Precompile(this ViewCompilationConfiguration compilationConfiguration, DotvvmConfiguration config, IStartupTracer startupTracer)
        {
            return Task.Run(async () => {
                var compilationService = config.ServiceProvider.GetService<IDotvvmViewCompilationService>();
                if (compilationConfiguration.BackgroundCompilationDelay != null)
                {
                    await Task.Delay(compilationConfiguration.BackgroundCompilationDelay.Value);
                }

                startupTracer.TraceEvent(StartupTracingConstants.ViewCompilationStarted);

                await compilationService.CompileAll(compilationConfiguration.CompileInParallel, false);

                startupTracer.TraceEvent(StartupTracingConstants.ViewCompilationFinished);
            });
        }

        public static void HandleViewCompilation(this ViewCompilationConfiguration compilationConfiguration, DotvvmConfiguration config, IStartupTracer startupTracer)
        {
            if (compilationConfiguration.Mode == ViewCompilationMode.Lazy)
                return;

            var getCompilationTask = compilationConfiguration.Precompile(config, startupTracer);
            if (compilationConfiguration.Mode == ViewCompilationMode.DuringApplicationStart)
            {
                getCompilationTask.Wait();
            }
        }
    }
}
