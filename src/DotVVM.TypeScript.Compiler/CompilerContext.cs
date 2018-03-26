using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler
{
    public struct CompilerContext
    {
        public Workspace Workspace { get; set; }
        public Compilation Compilation { get; set; }
    }
}
