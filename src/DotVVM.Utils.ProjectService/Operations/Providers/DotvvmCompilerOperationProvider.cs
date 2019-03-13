using DotVVM.Utils.ConfigurationHost.Extensions;
using DotVVM.Utils.ConfigurationHost.Lookup;
using DotVVM.Utils.ConfigurationHost.Operations.DotvvmCompiler;
using DotVVM.Utils.ConfigurationHost.Output.Statistics;

namespace DotVVM.Utils.ConfigurationHost.Operations.Providers
{
    public class DotvvmCompilerOperationProvider : IOperationProvider
    {
        private IOperation CompilerNet { get; }
        private IOperation CompilerCore { get; }
        private IOperation SkipCompiler { get; }

        public DotvvmCompilerOperationProvider(IStatisticsProvider statisticsProvider, AppConfiguration configuration)
        {
            SkipCompiler = new SkipDotvvmCompilerOperation(statisticsProvider);
            CompilerNet = string.IsNullOrWhiteSpace(configuration.DotvvmCompilerNetPath) ?
                SkipCompiler :
                new DotvvmCompilerNetOperation(statisticsProvider, configuration.DotvvmCompilerNetPath);

            CompilerCore = string.IsNullOrWhiteSpace(configuration.DotvvmCompilerCorePath) ?
                SkipCompiler :
                new DotvvmCompilerCoreOperation(statisticsProvider, configuration.DotvvmCompilerCorePath);
        }

        public IOperation GetOperation(IResult result)
        {
            if (!result.RunDotvvmCompiler) return SkipCompiler;
            switch (result.TargetFramework)
            {
                case TargetFramework.NetCore:
                    return CompilerCore;
                case TargetFramework.NetFramework:
                    return CompilerNet;
                default:
                    return SkipCompiler;
            }

        }
    }
}