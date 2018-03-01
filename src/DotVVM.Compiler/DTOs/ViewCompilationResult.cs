using System;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Compiler
{
    internal class ViewCompilationResult
    {
        public string BuilderClassName { get; set; }
        public Type ControlType { get; set; }
        public Type DataContextType { get; set; }
        public ResolvedTreeRoot ResolvedTree { get; set; }
    }
}