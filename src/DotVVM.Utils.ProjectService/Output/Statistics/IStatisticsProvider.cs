using DotVVM.Utils.ConfigurationHost.Operations;
using System.Collections.Generic;

namespace DotVVM.Utils.ConfigurationHost.Output.Statistics
{
    public interface IStatisticsProvider
    {
        void SaveStatistics(IEnumerable<IResult> results);
        void AddOperationResult(IResult searchResult, OperationResult operationResult);
        IEnumerable<IResult> TransformResults(IEnumerable<IResult> results);
        string GetDotvvmCompilerLogFileArgument(IResult result);
    }
}