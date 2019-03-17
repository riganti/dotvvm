using System;
using System.Diagnostics;
using System.Linq;
using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Operations.Providers;
using DotVVM.Utils.ProjectService.Output;
using DotVVM.Utils.ProjectService.Output.Statistics;

namespace DotVVM.Utils.ProjectService
{
    class Program
    {
        public static readonly IOutputLogger Logger = new AggregatedOutputLogger(new ConsoleOutputLogger());
        static void Main(string[] args)
        {
            try
            {
                Execute(args);
                WaitWhenDebuggerAttached();
            }
            catch (Exception e)
            {
                Logger.WriteError(e);
                WaitWhenDebuggerAttached();
                Environment.Exit(1);
            }
        }

        private static void WaitWhenDebuggerAttached()
        {
            if (Debugger.IsAttached)
            {
                Console.Write("Continue by pressing key...");
                Console.ReadKey();
            }
        }

        private static void Execute(string[] args)
        {
            var configuration = new DotvvmProjectSertviceConfiguration();
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
