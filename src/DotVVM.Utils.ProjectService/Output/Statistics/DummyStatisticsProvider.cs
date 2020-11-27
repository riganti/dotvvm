using System.Collections.Generic;
using DotVVM.Utils.ProjectService.Operations;

namespace DotVVM.Utils.ProjectService.Output.Statistics
{
    public class DummyStatisticsProvider : IStatisticsProvider
    {
        public void SaveStatistics(IEnumerable<IResolvedProjectMetadata> results)
        {
        }

        public void AddOperationResult(IResolvedProjectMetadata resolvedMetadata, OperationResult operationResult)
        {
        }

        public IEnumerable<IResolvedProjectMetadata> TransformResults(IEnumerable<IResolvedProjectMetadata> results)
        {
            return results;
        }

        public string GetDotvvmCompilerLogFileArgument(IResolvedProjectMetadata metadata)
        {
            return string.Empty;
        }
    }
}