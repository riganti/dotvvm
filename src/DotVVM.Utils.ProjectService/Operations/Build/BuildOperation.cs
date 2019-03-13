using System;
using DotVVM.Utils.ConfigurationHost.Output;

namespace DotVVM.Utils.ConfigurationHost.Operations.Build
{
    public abstract class BuildOperation : CommandOperationBase
    {
        public sealed override string OperationName { get; set; } = "build";
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
            operationResult.Successful = RunBuild(logger, arguments);
            return operationResult;
        }

        protected abstract string ComposeArguments(IResult result);

        protected abstract bool RunBuild(IOutputLogger logger, string arguments);
    }
}