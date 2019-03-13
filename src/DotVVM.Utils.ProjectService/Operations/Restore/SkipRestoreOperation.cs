using System;
using DotVVM.Utils.ConfigurationHost.Output;

namespace DotVVM.Utils.ConfigurationHost.Operations.Restore
{
    public class SkipRestoreOperation : RestoreOperation
    {
        public override OperationResult Execute(IResult result, IOutputLogger logger)
        {
            logger.WriteInfo($"Skipped restoring project: {result.CsprojFullName}");
            return new OperationResult() {OperationName = OperationName};
        }

        protected override string ComposeArguments(IResult result)
        {
            return null;
        }

        protected override bool RunRestore(IOutputLogger logger, string arguments)
        {
            return false;
        }
    }
}