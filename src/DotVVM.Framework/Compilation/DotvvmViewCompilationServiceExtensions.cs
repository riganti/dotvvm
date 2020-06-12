using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Compilation
{
    public static class DotvvmViewCompilationServiceExtensions
    {
        public static Task Precompile(this ViewCompilationConfiguration compilationConfiguration,
            DotvvmConfiguration config)
        {
            return Task.Run(async () => {
                var compilationService = config.ServiceProvider.GetService<IDotvvmViewCompilationService>();
                if (compilationConfiguration.BackgroundCompilationDelay != null)
                {
                    await Task.Delay(compilationConfiguration.BackgroundCompilationDelay.Value);
                }

                await compilationService.CompileAll(
                    compilationConfiguration.CompileInParallel, false);
            });
        }

        public static void HandleViewCompilation(this ViewCompilationConfiguration compilationConfiguration, DotvvmConfiguration config)
        {
            if (compilationConfiguration.Mode == ViewCompilationMode.Lazy)
                return;

            var getCompilationTask = compilationConfiguration.Precompile(config);
            if (compilationConfiguration.Mode == ViewCompilationMode.DuringApplicationStart)
            {
                getCompilationTask.Wait();
            }
        }
    }
}
