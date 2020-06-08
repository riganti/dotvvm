using System.Collections.Generic;
using DotVVM.Utils.ProjectService.Operations;

namespace DotVVM.Utils.ProjectService.Output.Statistics
{
    public interface IStatisticsProvider
    {
        void SaveStatistics(IEnumerable<IResolvedProjectMetadata> results);
        void AddOperationResult(IResolvedProjectMetadata resolvedMetadata, OperationResult operationResult);
        IEnumerable<IResolvedProjectMetadata> TransformResults(IEnumerable<IResolvedProjectMetadata> results);
        string GetDotvvmCompilerLogFileArgument(IResolvedProjectMetadata metadata);
    }
}