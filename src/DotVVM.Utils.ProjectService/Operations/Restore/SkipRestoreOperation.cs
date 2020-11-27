using DotVVM.Utils.ProjectService.Output;

namespace DotVVM.Utils.ProjectService.Operations.Restore
{
    public class SkipRestoreOperation : RestoreOperation
    {
        public override OperationResult Execute(IResolvedProjectMetadata metadata, IOutputLogger logger)
        {
            logger.WriteInfo($"Skipped restoring project: {metadata.CsprojFullName}");
            return new OperationResult() {OperationName = OperationName};
        }

        protected override string ComposeArguments(IResolvedProjectMetadata metadata)
        {
            return null;
        }

        protected override bool RunRestore(IOutputLogger logger, string arguments)
        {
            return false;
        }
    }
}