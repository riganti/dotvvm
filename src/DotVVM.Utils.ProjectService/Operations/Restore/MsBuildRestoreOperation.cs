using System;
using DotVVM.Utils.ConfigurationHost.Lookup;
using DotVVM.Utils.ConfigurationHost.Output;

namespace DotVVM.Utils.ConfigurationHost.Operations.Restore
{
    public class MsBuildRestoreOperation : RestoreOperation
    {
        private string MsBuildPath { get; }

        public MsBuildRestoreOperation(string msBuildPath)
        {
            SupportedCsprojVersion = CsprojVersion.OlderProjectSystem;
            MsBuildPath = msBuildPath;
        }

        protected override string ComposeArguments(IResult result)
        {
            return $" \"{result.CsprojFullName}\" /v:m /t:restore";
        }

        protected override bool RunRestore(IOutputLogger logger, string arguments)
        {
            return RunCommand(logger, MsBuildPath, arguments);
        }
    }
}