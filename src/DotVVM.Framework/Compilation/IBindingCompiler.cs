#nullable enable
using System;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Compilation
{
    public interface IBindingCompiler
    {
        ExpressionSyntax EmitCreateBinding(DefaultViewCompilerCodeEmitter emitter, ResolvedBinding binding);
    }
}
