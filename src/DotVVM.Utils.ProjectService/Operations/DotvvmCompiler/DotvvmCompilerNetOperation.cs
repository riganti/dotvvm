using DotVVM.Utils.ProjectService.Output;
using DotVVM.Utils.ProjectService.Output.Statistics;

namespace DotVVM.Utils.ProjectService.Operations.DotvvmCompiler
{
    public class DotvvmCompilerNetOperation : DotvvmCompilerOperation
    {
        public DotvvmCompilerNetOperation(IStatisticsProvider statisticsProvider, string compilerPath) : base(
            statisticsProvider, compilerPath)
        {
        }

        public override bool RunCompiler(IOutputLogger logger, IResolvedProjectMetadata metadata, string arguments)
        {
            return RunCommand(logger, CompilerPath, arguments);
        }
    }
}