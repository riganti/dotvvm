using System;
using System.Diagnostics;
using DotVVM.Utils.ConfigurationHost.Extensions;
using DotVVM.Utils.ConfigurationHost.Output;
using DotVVM.Utils.ConfigurationHost.Output.Statistics;

namespace DotVVM.Utils.ConfigurationHost.Operations.DotvvmCompiler
{
    public class DotvvmCompilerCoreOperation : DotvvmCompilerOperation
    {
        public DotvvmCompilerCoreOperation(IStatisticsProvider statisticsProvider, string compilerPath) : base(statisticsProvider, compilerPath)
        {
        }

        public override bool RunCompiler(IOutputLogger logger, IResult result, string arguments)
        {
            return RunCommand(logger, new ProcessStartInfo(CompilerPath, arguments)
            {
                EnvironmentVariables =
                {
                    ["webAssemblyPath"] = result.GetWebsiteAssemblyPath(),
                    ["assemblySearchPath"] = result.GetWebsiteRootPath()
                }
            });
        }
    }
}