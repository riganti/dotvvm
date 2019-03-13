using DotVVM.Utils.ConfigurationHost.Output;
using DotVVM.Utils.ConfigurationHost.Output.Statistics;

namespace DotVVM.Utils.ConfigurationHost.Operations.DotvvmCompiler
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