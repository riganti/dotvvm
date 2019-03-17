using DotVVM.Utils.ProjectService.Output;
using DotVVM.Utils.ProjectService.Output.Statistics;

namespace DotVVM.Utils.ProjectService.Operations.DotvvmCompiler
{
    public class SkipDotvvmCompilerOperation : DotvvmCompilerOperation
    {
        public SkipDotvvmCompilerOperation(IStatisticsProvider statisticsProvider) : base(statisticsProvider, "")
        {
        }

        public override OperationResult Execute(IResult result, IOutputLogger logger)
        {
            logger.WriteInfo($"Skipped dotvvm compiling of project: {result.CsprojFullName}");
            return new OperationResult() {OperationName = OperationName};
        }

        public override bool RunCompiler(IOutputLogger logger, IResult result, string arguments)
        {
            return false;
        }
    }
}