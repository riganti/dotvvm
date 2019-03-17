using System;
using DotVVM.Utils.ProjectService.Lookup;

namespace DotVVM.Utils.ProjectService.Operations.DotvvmCompiler
{
    public class AssemblyPreprocessorFactory
    {
        public IAssemblyPreprocessor GetAssemblyPreprocessor(IResult result, string compilerPath)
        {
            switch (result.CsprojVersion)
            {
                case CsprojVersion.OlderProjectSystem:
                    return new AssemblyPreprocessorOldCsproj(result, compilerPath);
                case CsprojVersion.DotNetSdk when result.TargetFramework == TargetFramework.NetCore:
                    return new AssemblyPreprocessorNetSdkCore(result, compilerPath);
                case CsprojVersion.DotNetSdk when result.TargetFramework == TargetFramework.NetFramework:
                    return new AssemblyPreprocessorNetSdkNet(result, compilerPath);
                default:
                    throw new Exception("Project for dotvvm compilation must have valid project system.");
            }
        }
    }
}