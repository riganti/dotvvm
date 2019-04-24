using DotVVM.Utils.ProjectService.Output;
using DotVVM.Utils.ProjectService.Output.Statistics;

namespace DotVVM.Utils.ProjectService.Operations.DotvvmCompiler
{
    public class SkipDotvvmCompilerOperation : DotvvmCompilerOperation
    {
        public SkipDotvvmCompilerOperation(IStatisticsProvider statisticsProvider) : base(statisticsProvider, "")
        {
        }

        public override OperationResult Execute(IResolvedProjectMetadata metadata, IOutputLogger logger)
        {
            logger.WriteInfo($"Skipped dotvvm compiling of project: {metadata.CsprojFullName}");
            return new OperationResult() {OperationName = OperationName};
        }

        public override bool RunCompiler(IOutputLogger logger, IResolvedProjectMetadata metadata, string arguments)
        {
            return false;
        }
    }
}