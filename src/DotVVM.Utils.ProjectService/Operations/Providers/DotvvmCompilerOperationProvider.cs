using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Operations.DotvvmCompiler;
using DotVVM.Utils.ProjectService.Output.Statistics;

namespace DotVVM.Utils.ProjectService.Operations.Providers
{
    public class DotvvmCompilerOperationProvider : IOperationProvider
    {
        private IOperation CompilerNet { get; }
        private IOperation CompilerCore { get; }
        private IOperation SkipCompiler { get; }

        public DotvvmCompilerOperationProvider(IStatisticsProvider statisticsProvider, DotvvmToolMetadata metadata)
        {
            SkipCompiler = new SkipDotvvmCompilerOperation(statisticsProvider);
            CompilerNet = string.IsNullOrWhiteSpace(metadata.MainModulePath) ?
                SkipCompiler :
                new DotvvmCompilerNetOperation(statisticsProvider, metadata.MainModulePath);

            CompilerCore = string.IsNullOrWhiteSpace(metadata.MainModulePath) ?
                SkipCompiler :
                new DotvvmCompilerCoreOperation(statisticsProvider, metadata.MainModulePath);
        }

        public IOperation GetOperation(IResolvedProjectMetadata metadata)
        {
            if (!metadata.RunDotvvmCompiler) return SkipCompiler;
            switch (metadata.TargetFramework)
            {
                case TargetFramework.NetStandard:
                    return CompilerCore;
                case TargetFramework.NetFramework:
                    return CompilerNet;
                default:
                    return SkipCompiler;
            }

        }
    }
}
