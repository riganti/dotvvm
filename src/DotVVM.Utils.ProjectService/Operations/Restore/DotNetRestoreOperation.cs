using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Output;

namespace DotVVM.Utils.ProjectService.Operations.Restore
{
    public class DotNetRestoreOperation : RestoreOperation
    {
        public DotNetRestoreOperation()
        {
            SupportedCsprojVersion = CsprojVersion.DotNetSdk;
        }

        protected override string ComposeArguments(IResolvedProjectMetadata metadata)
        {
            return $" {OperationName} \"{metadata.CsprojFullName}\" -v m";
        }

        protected override bool RunRestore(IOutputLogger logger, string arguments)
        {
            return RunCommand(logger, "dotnet", arguments);
        }
    }
}