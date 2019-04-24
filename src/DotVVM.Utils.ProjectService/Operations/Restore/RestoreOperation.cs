using DotVVM.Utils.ProjectService.Output;

namespace DotVVM.Utils.ProjectService.Operations.Restore
{
    public abstract class RestoreOperation : CommandOperationBase
    {
        public sealed override string OperationName { get; set; } = "restore";
        
        public override OperationResult Execute(IResolvedProjectMetadata metadata, IOutputLogger logger)
        {
            VerifyCsprojVersion(metadata);

            var operationResult = new OperationResult()
            {
                OperationName = OperationName
            };

            logger.WriteInfo($"{OperationName} project: {metadata.CsprojFullName}");

            operationResult.Executed = true;
            var arguments = ComposeArguments(metadata);
            operationResult.Successful = RunRestore(logger,arguments);
            return operationResult;
        }

        protected abstract string ComposeArguments(IResolvedProjectMetadata metadata);

        protected abstract bool RunRestore(IOutputLogger logger, string arguments);
    }
}