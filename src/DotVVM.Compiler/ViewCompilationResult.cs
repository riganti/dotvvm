using System;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Compiler
{
    public class ViewCompilationResult
    {
        public string BuilderClassName { get; set; }
        public Type ControlType { get; set; }
        public Type DataContextType { get; set; }
        public ResolvedTreeRoot ResolvedTree { get; set; }
    }
}
