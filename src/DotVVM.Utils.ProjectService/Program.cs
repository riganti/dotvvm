using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using DotVVM.Utils.ConfigurationHost.Lookup;
using DotVVM.Utils.ConfigurationHost.Operations;
using DotVVM.Utils.ConfigurationHost.Operations.DotvvmCompiler;
using DotVVM.Utils.ConfigurationHost.Operations.Providers;
using DotVVM.Utils.ConfigurationHost.Output;
using DotVVM.Utils.ConfigurationHost.Output.Statistics;

namespace DotVVM.Utils.ConfigurationHost
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
            var configuration = new AppConfiguration();
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
