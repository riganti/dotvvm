#nullable enable
using System;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using Microsoft.CodeAnalysis.CSharp;

namespace DotVVM.Framework.Compilation
{
    /// <summary>
    /// Gets the Dothtml view and compiles it into a function.
    /// </summary>
    public interface IViewCompiler
    {
        /// <summary>
        /// Compiles the view and returns a function that can be invoked repeatedly. The function builds full control tree and activates the page.
        /// </summary>
        (ControlBuilderDescriptor, Func<IControlBuilder>) CompileView(string sourceCode, MarkupFile file, string assemblyName, string namespaceName, string className);

    }
}
