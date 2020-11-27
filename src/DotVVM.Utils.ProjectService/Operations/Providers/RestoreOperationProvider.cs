using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Operations.Restore;

namespace DotVVM.Utils.ProjectService.Operations.Providers
{
    public class RestoreOperationProvider : IOperationProvider
    {
        private DotNetRestoreOperation DotNetRestore { get; }
        private MsBuildRestoreOperation MsBuildRestore { get; }
        private SkipRestoreOperation SkipRestore { get; }

        public RestoreOperationProvider(string msbuildPath)
        {
            DotNetRestore = new DotNetRestoreOperation();
            MsBuildRestore = new MsBuildRestoreOperation(msbuildPath);
            SkipRestore = new SkipRestoreOperation();
        }

        public IOperation GetOperation(IResolvedProjectMetadata metadata)
        {
            switch (metadata.CsprojVersion)
            {
                case CsprojVersion.DotNetSdk:
                    return DotNetRestore;
                case CsprojVersion.OlderProjectSystem:
                    return MsBuildRestore;
                default:
                    return SkipRestore;
            }

        }
    }
}
