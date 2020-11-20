using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Output;

namespace DotVVM.Utils.ProjectService.Operations.Build
{
    public class DotNetBuildOperation : BuildOperation
    {
        public DotNetBuildOperation()
        {
            SupportedCsprojVersion = CsprojVersion.DotNetSdk;
        }

        protected override string ComposeArguments(IResolvedProjectMetadata metadata)
        {
            return $" {OperationName} \"{metadata.CsprojFullName}\" -v m --output {Constants.BuildPath}";
        }

        protected override bool RunBuild(IOutputLogger logger, string arguments)
        {
            return RunCommand(logger, "dotnet", arguments);
        }
    }
}