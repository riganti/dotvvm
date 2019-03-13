using System;
using DotVVM.Utils.ConfigurationHost.Lookup;
using DotVVM.Utils.ConfigurationHost.Output;

namespace DotVVM.Utils.ConfigurationHost.Operations.Build
{
    public class MsBuildBuildOperation : BuildOperation
    {
        private string MsBuildPath { get; }

        public MsBuildBuildOperation(string msBuildPath)
        {
            SupportedCsprojVersion = CsprojVersion.OlderProjectSystem;
            MsBuildPath = msBuildPath;
        }

        protected override string ComposeArguments(IResult result)
        {
            return $" \"{result.CsprojFullName}\" /restore /v:m /p:OutDir={Constants.BuildPath}";
        }

        protected override bool RunBuild(IOutputLogger logger, string arguments)
        {
            return RunCommand(logger, MsBuildPath, arguments);
        }
    }
}