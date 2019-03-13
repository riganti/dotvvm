using DotVVM.Utils.ConfigurationHost.Lookup;
using DotVVM.Utils.ConfigurationHost.Operations.Restore;

namespace DotVVM.Utils.ConfigurationHost.Operations.Providers
{
    public class RestoreOperationProvider : IOperationProvider
    {
        private DotNetRestoreOperation DotNetRestore { get; }
        private MsBuildRestoreOperation MsBuildRestore { get; }
        private SkipRestoreOperation SkipRestore { get; }

        public RestoreOperationProvider(AppConfiguration configuration)
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