using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Compilation
{
    public static class DotvvmViewCompilationServiceExtensions
    {
        public static void Precompile(this ViewCompilationConfiguration compilationConfiguration, DotvvmConfiguration config)
        {
            var compilationTask = new Task(async () => {
                var compilationService = config.ServiceProvider.GetService<IDotvvmViewCompilationService>();
                if (compilationConfiguration.BackgroundCompilationDelay != null)
                {
                    await Task.Delay(compilationConfiguration.BackgroundCompilationDelay.Value);
                }

                await compilationService.CompileAll(
                    compilationConfiguration.Mode == ViewCompilationMode.ParallelPrecompilation, false);
            }, TaskCreationOptions.LongRunning);

            compilationTask.ConfigureAwait(false);
            compilationTask.Start();
        }
    }
}
