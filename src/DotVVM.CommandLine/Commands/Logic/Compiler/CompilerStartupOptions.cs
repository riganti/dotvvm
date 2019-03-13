using DotVVM.Compiler;

namespace DotVVM.CommandLine.Commands.Logic.Compiler
{
    public class CompilerStartupOptions
    {
        public CompilerOptions Options { get; set; }

        public string CompilerExePath { get; set; } = @"C:\dev\dotvvm\src\DotVVM.Compiler\bin\Debug\net461\DotVVM.Compiler.exe";
        public bool WaitForDebugger { get; set; }
        public bool WaitForDebuggerAndBreak { get; set; }
    }
}
