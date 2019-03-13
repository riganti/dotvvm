using DotVVM.Utils.ConfigurationHost.Extensions;
using DotVVM.Utils.ConfigurationHost.Lookup;
using DotVVM.Utils.ConfigurationHost.Output;
using DotVVM.Utils.ConfigurationHost.Output.Statistics;
using System;

namespace DotVVM.Utils.ConfigurationHost.Operations.DotvvmCompiler
{
    public class DotvvmCompilerNetOperation : DotvvmCompilerOperation
    {
        public DotvvmCompilerNetOperation(IStatisticsProvider statisticsProvider, string compilerPath) : base(
            statisticsProvider, compilerPath)
        {
        }

        public override bool RunCompiler(IOutputLogger logger, IResult result, string arguments)
        {
            return RunCommand(logger, CompilerPath, arguments);
        }
    }
}