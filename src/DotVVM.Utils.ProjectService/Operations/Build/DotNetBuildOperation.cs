using System;
using DotVVM.Utils.ConfigurationHost.Lookup;
using DotVVM.Utils.ConfigurationHost.Output;

namespace DotVVM.Utils.ConfigurationHost.Operations.Build
{
    public class DotNetBuildOperation : BuildOperation
    {
        public DotNetBuildOperation()
        {
            SupportedCsprojVersion = CsprojVersion.DotNetSdk;
        }

        protected override string ComposeArguments(IResult result)
        {
            return $" {OperationName} \"{result.CsprojFullName}\" -v m --output {Constants.BuildPath}";
        }

        protected override bool RunBuild(IOutputLogger logger, string arguments)
        {
            return RunCommand(logger, "dotnet", arguments);
        }
    }
}