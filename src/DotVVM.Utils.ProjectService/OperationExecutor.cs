using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Utils.ProjectService.Operations;
using DotVVM.Utils.ProjectService.Operations.Providers;
using DotVVM.Utils.ProjectService.Output;
using DotVVM.Utils.ProjectService.Output.Statistics;

namespace DotVVM.Utils.ProjectService
{
    public class OperationExecutor
    {
        private IOutputLogger Logger { get; }
        public IStatisticsProvider StatisticsProvider { get; }
        public IEnumerable<IResolvedProjectMetadata> Results { get; }

        public OperationExecutor(IEnumerable<IResolvedProjectMetadata> results, IOutputLogger logger,
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