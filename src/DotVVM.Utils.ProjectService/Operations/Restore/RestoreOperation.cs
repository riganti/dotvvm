using DotVVM.Utils.ProjectService.Output;

namespace DotVVM.Utils.ProjectService.Operations.Restore
{
    public abstract class RestoreOperation : CommandOperationBase
    {
        public sealed override string OperationName { get; set; } = "restore";
        
        public override OperationResult Execute(IResult result, IOutputLogger logger)
        {
            VerifyCsprojVersion(result);

            var operationResult = new OperationResult()
            {
                OperationName = OperationName
            };

            logger.WriteInfo($"{OperationName} project: {result.CsprojFullName}");

            operationResult.Executed = true;
            var arguments = ComposeArguments(result);
            operationResult.Successful = RunRestore(logger,arguments);
            return operationResult;
        }

        protected abstract string ComposeArguments(IResult result);

        protected abstract bool RunRestore(IOutputLogger logger, string arguments);
    }
}