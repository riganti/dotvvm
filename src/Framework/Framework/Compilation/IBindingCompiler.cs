using System;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.ViewCompiler;

namespace DotVVM.Framework.Compilation
{
    public interface IBindingCompiler
    {
        Expression EmitCreateBinding(DefaultViewCompilerCodeEmitter emitter, ResolvedBinding binding);
    }
}
