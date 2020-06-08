using System;
using DotVVM.Utils.ProjectService.Lookup;

namespace DotVVM.Utils.ProjectService.Operations.DotvvmCompiler
{
    public class AssemblyPreprocessorFactory
    {
        public IAssemblyPreprocessor GetAssemblyPreprocessor(IResolvedProjectMetadata metadata, string compilerPath)
        {
            switch (metadata.CsprojVersion)
            {
                case CsprojVersion.OlderProjectSystem:
                    return new AssemblyPreprocessorOldCsproj(metadata, compilerPath);
                case CsprojVersion.DotNetSdk when (metadata.TargetFramework & TargetFramework.NetStandard) > 0:
                    return new AssemblyPreprocessorNetSdkCore(metadata, compilerPath);
                case CsprojVersion.DotNetSdk when (metadata.TargetFramework & TargetFramework.NetFramework) > 0:
                    return new AssemblyPreprocessorNetSdkNet(metadata, compilerPath);
                default:
                    throw new Exception("Project for a DotVVM compilation must have valid project system.");
            }
        }
    }
}
