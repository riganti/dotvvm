using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Output;

namespace DotVVM.Utils.ProjectService.Operations.Restore
{
    public class MsBuildRestoreOperation : RestoreOperation
    {
        private string MsBuildPath { get; }

        public MsBuildRestoreOperation(string msBuildPath)
        {
            SupportedCsprojVersion = CsprojVersion.OlderProjectSystem;
            MsBuildPath = msBuildPath;
        }

        protected override string ComposeArguments(IResolvedProjectMetadata metadata)
        {
            return $" \"{metadata.CsprojFullName}\" /v:m /t:restore";
        }

        protected override bool RunRestore(IOutputLogger logger, string arguments)
        {
            return RunCommand(logger, MsBuildPath, arguments);
        }
    }
}