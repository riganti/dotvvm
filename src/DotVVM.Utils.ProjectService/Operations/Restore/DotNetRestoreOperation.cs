using System;
using DotVVM.Utils.ConfigurationHost.Lookup;
using DotVVM.Utils.ConfigurationHost.Output;

namespace DotVVM.Utils.ConfigurationHost.Operations.Restore
{
    public class DotNetRestoreOperation : RestoreOperation
    {
        public DotNetRestoreOperation()
        {
            SupportedCsprojVersion = CsprojVersion.DotNetSdk;
        }

        protected override string ComposeArguments(IResult result)
        {
            return $" {OperationName} \"{result.CsprojFullName}\" -v m";
        }

        protected override bool RunRestore(IOutputLogger logger, string arguments)
        {
            return RunCommand(logger, "dotnet", arguments);
        }
    }
}