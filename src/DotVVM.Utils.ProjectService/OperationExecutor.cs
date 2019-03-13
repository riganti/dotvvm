using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Utils.ConfigurationHost.Extensions;
using DotVVM.Utils.ConfigurationHost.Lookup;
using DotVVM.Utils.ConfigurationHost.Operations;
using DotVVM.Utils.ConfigurationHost.Operations.Providers;
using DotVVM.Utils.ConfigurationHost.Output;
using DotVVM.Utils.ConfigurationHost.Output.Statistics;

namespace DotVVM.Utils.ConfigurationHost
{
    public class OperationExecutor
    {
        private IOutputLogger Logger { get; }
        public IStatisticsProvider StatisticsProvider { get; }
        public IEnumerable<IResult> Results { get; }

        public OperationExecutor(IEnumerable<IResult> results, IOutputLogger logger,
            IStatisticsProvider statisticsProvider)
        {
            Logger = logger;
            StatisticsProvider = statisticsProvider;
            Results = StatisticsProvider.TransformResults(results).ToList();
        }

        public void Execute(bool condition, IOperationProvider provider)
        {
            if (!condition) return;
            
            foreach (var searchResult in Results)
            {
                var operation = provider.GetOperation(searchResult);
                OperationResult operationResult; 
                try
                {
                    operationResult = operation.Execute(searchResult, Logger);
                }
                catch (Exception e)
                {
                    Logger.WriteError($"Operation {operation.OperationName} has failed. Project: {searchResult.CsprojFullName}");
                    Logger.WriteError(e);
                    operationResult = new OperationResult()
                    {
                        OperationName = operation.OperationName,
                        Executed = true
                    };
                }

                StatisticsProvider.AddOperationResult(searchResult, operationResult);
            }
        }

    }
}