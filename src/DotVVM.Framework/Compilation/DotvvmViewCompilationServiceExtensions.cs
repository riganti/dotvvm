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

        public static void HandleViewCompilation(this ViewCompilationConfiguration compilationConfiguration,
            DotvvmConfiguration config)
        {
            var getCompilationTask = compilationConfiguration.Precompile(config);
            if (compilationConfiguration.Mode == ViewCompilationMode.AfterStartup)
            {
#pragma warning disable 4014
                getCompilationTask.ConfigureAwait(false);
#pragma warning restore 4014
            }
            else if (compilationConfiguration.Mode == ViewCompilationMode.DuringStartup)
            {
                getCompilationTask.Wait();
            }
        }
    }
}
