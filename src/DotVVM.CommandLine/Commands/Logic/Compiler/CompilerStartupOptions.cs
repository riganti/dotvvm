using DotVVM.Compiler;

namespace DotVVM.CommandLine.Commands.Logic.Compiler
{
    public class CompilerStartupOptions
    {
        public CompilerOptions Options { get; set; }
        public bool WaitForDebugger { get; set; }
        public bool WaitForDebuggerAndBreak { get; set; }
    }
}
