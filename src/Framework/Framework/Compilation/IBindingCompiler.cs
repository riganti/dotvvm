using System;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Compilation
{
    public interface IBindingCompiler
    {
        Expression EmitCreateBinding(DefaultViewCompilerCodeEmitter emitter, ResolvedBinding binding);
    }
}
