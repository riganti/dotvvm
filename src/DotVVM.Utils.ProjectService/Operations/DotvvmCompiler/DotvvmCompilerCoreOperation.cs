using System.Diagnostics;
using DotVVM.Utils.ProjectService.Extensions;
using DotVVM.Utils.ProjectService.Output;
using DotVVM.Utils.ProjectService.Output.Statistics;

namespace DotVVM.Utils.ProjectService.Operations.DotvvmCompiler
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
