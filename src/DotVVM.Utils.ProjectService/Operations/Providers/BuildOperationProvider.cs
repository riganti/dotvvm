using System;
using DotVVM.Utils.ConfigurationHost.Lookup;
using DotVVM.Utils.ConfigurationHost.Operations.Build;

namespace DotVVM.Utils.ConfigurationHost.Operations.Providers
{
    public class BuildOperationProvider : IOperationProvider
    {
        private DotNetBuildOperation DotNetBuild { get; }
        private MsBuildBuildOperation MsBuild { get; }
        private SkipBuildOperation SkipBuild { get; }

        public BuildOperationProvider(AppConfiguration configuration)
        {
            DotNetBuild = new DotNetBuildOperation();
            SkipBuild = new SkipBuildOperation();
            MsBuild = new MsBuildBuildOperation(configuration.MsBuildPath);
        }

        public IOperation GetOperation(IResult result)
        {
            switch (result.CsprojVersion)
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