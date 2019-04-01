using System.Collections.Generic;
using DotVVM.Utils.ProjectService.Operations;

namespace DotVVM.Utils.ProjectService.Output.Statistics
{
    public interface IStatisticsProvider
    {
        void SaveStatistics(IEnumerable<IResult> results);
        void AddOperationResult(IResult searchResult, OperationResult operationResult);
        IEnumerable<IResult> TransformResults(IEnumerable<IResult> results);
        string GetDotvvmCompilerLogFileArgument(IResult result);
    }
}