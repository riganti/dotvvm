using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Compilation.ViewCompiler
{
    public class ViewCompilerConfiguration
    {
        public List<Func<ResolvedControlTreeVisitor>> TreeVisitors { get; } = new List<Func<ResolvedControlTreeVisitor>>();
    }
}
