using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler
{
    internal struct CompilerContext
    {
        public Workspace Workspace { get; set; }
        public Compilation Compilation { get; set; }
    }
}