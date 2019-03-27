using System;
using System.Diagnostics;
using System.Linq;
using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Operations.Providers;
using DotVVM.Utils.ProjectService.Output;
using DotVVM.Utils.ProjectService.Output.Statistics;

namespace DotVVM.Utils.ProjectService
{
    internal class Program
    {
        public readonly IOutputLogger Logger = new AggregatedOutputLogger(new ConsoleOutputLogger());


        private void Execute()
        {
            var configuration = new ProjectServiceConfiguration();
            var results = new ProjectSystemSearcher().Search(configuration).ToList();

            var statisticsProvider = new StatisticsProviderFactory().GetProvider(configuration);
            var executor = new OperationExecutor(results, Logger, statisticsProvider);

            executor.Execute(configuration.Restore, new RestoreOperationProvider(configuration));
            executor.Execute(configuration.Build, new BuildOperationProvider(configuration));
            executor.Execute(configuration.DotvvmCompiler, new DotvvmCompilerOperationProvider(statisticsProvider, configuration));

            statisticsProvider.SaveStatistics(executor.Results);
        }
    }
}
