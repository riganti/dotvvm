using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Operations.Build;

namespace DotVVM.Utils.ProjectService.Operations.Providers
{
    public class BuildOperationProvider : IOperationProvider
    {
        private DotNetBuildOperation DotNetBuild { get; }
        private MsBuildBuildOperation MsBuild { get; }
        private SkipBuildOperation SkipBuild { get; }

        public BuildOperationProvider(string msbuildPath)
        {
            DotNetBuild = new DotNetBuildOperation();
            SkipBuild = new SkipBuildOperation();
            MsBuild = new MsBuildBuildOperation(msbuildPath);
        }

        public IOperation GetOperation(IResolvedProjectMetadata metadata)
        {
            switch (metadata.CsprojVersion)
            {
                case CsprojVersion.DotNetSdk:
                    return DotNetBuild;
                case CsprojVersion.OlderProjectSystem:
                    return MsBuild;
                default:
                    return SkipBuild;
            }

        }
    }
}
