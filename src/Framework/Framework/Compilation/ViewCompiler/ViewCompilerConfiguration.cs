using System;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using System.Collections.Generic;

namespace DotVVM.Framework.Compilation
{
    public class ViewCompilerConfiguration
    {
        public List<Func<ResolvedControlTreeVisitor>> TreeVisitors { get; } = new List<Func<ResolvedControlTreeVisitor>>();
    }
}
