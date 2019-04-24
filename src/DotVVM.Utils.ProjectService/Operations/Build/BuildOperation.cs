using DotVVM.Utils.ProjectService.Output;

namespace DotVVM.Utils.ProjectService.Operations.Build
{
    public abstract class BuildOperation : CommandOperationBase
    {
        public sealed override string OperationName { get; set; } = "build";
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
            operationResult.Successful = RunBuild(logger, arguments);
            return operationResult;
        }

        protected abstract string ComposeArguments(IResolvedProjectMetadata metadata);

        protected abstract bool RunBuild(IOutputLogger logger, string arguments);
    }
}