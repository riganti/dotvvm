using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Operations.Restore;

namespace DotVVM.Utils.ProjectService.Operations.Providers
{
    public class RestoreOperationProvider : IOperationProvider
    {
        private DotNetRestoreOperation DotNetRestore { get; }
        private MsBuildRestoreOperation MsBuildRestore { get; }
        private SkipRestoreOperation SkipRestore { get; }

        public RestoreOperationProvider(ProjectServiceConfiguration configuration)
        {
            DotNetRestore = new DotNetRestoreOperation();
            MsBuildRestore = new MsBuildRestoreOperation(configuration.MsBuildPath);
            SkipRestore = new SkipRestoreOperation();
        }

        public IOperation GetOperation(IResult result)
        {
            switch (result.CsprojVersion)
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