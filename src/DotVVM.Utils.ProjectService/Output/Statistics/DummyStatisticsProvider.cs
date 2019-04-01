using System.Collections.Generic;
using DotVVM.Utils.ProjectService.Operations;

namespace DotVVM.Utils.ProjectService.Output.Statistics
{
    public class DummyStatisticsProvider : IStatisticsProvider
    {
        public void SaveStatistics(IEnumerable<IResult> results)
        {
        }

        public void AddOperationResult(IResult searchResult, OperationResult operationResult)
        {
        }

        public IEnumerable<IResult> TransformResults(IEnumerable<IResult> results)
        {
            return results;
        }

        public string GetDotvvmCompilerLogFileArgument(IResult result)
        {
            return string.Empty;
        }
    }
}