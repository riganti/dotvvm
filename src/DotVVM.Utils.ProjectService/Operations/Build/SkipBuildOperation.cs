using System;
using DotVVM.Utils.ConfigurationHost.Output;

namespace DotVVM.Utils.ConfigurationHost.Operations.Build
{
    public class SkipBuildOperation : BuildOperation
    {
        public override OperationResult Execute(IResult result, IOutputLogger logger)
        {
            logger.WriteInfo($"Skipped building project: {result.CsprojFullName}");
            return new OperationResult() {OperationName = OperationName};
        }

        protected override string ComposeArguments(IResult result)
        {
            return null;
        }

        protected override bool RunBuild(IOutputLogger logger, string arguments)
        {
            return false;
        }
    }
}