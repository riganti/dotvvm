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
            var results = new ProjectSystemProvider().GetProjectMetadata(Environment.CurrentDirectory).ToList();

            var statisticsProvider = new StatisticsProviderFactory().GetProvider("folder");
            var executor = new OperationExecutor(results, Logger, statisticsProvider);

            executor.Execute(false, new RestoreOperationProvider(""));
            executor.Execute(false, new BuildOperationProvider(""));
            executor.Execute(false, new DotvvmCompilerOperationProvider(statisticsProvider, new DotvvmToolMetadata()));

            statisticsProvider.SaveStatistics(executor.Results);
        }
    }
}
