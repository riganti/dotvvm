using DotVVM.Utils.ProjectService.Output;

namespace DotVVM.Utils.ProjectService.Operations.Build
{
    public class SkipBuildOperation : BuildOperation
    {
        public override OperationResult Execute(IResolvedProjectMetadata metadata, IOutputLogger logger)
        {
            logger.WriteInfo($"Skipped building project: {metadata.CsprojFullName}");
            return new OperationResult() {OperationName = OperationName};
        }

        protected override string ComposeArguments(IResolvedProjectMetadata metadata)
        {
            return null;
        }

        protected override bool RunBuild(IOutputLogger logger, string arguments)
        {
            return false;
        }
    }
}