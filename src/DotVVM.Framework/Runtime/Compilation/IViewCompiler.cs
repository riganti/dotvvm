using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Parser;
using Microsoft.CodeAnalysis.CSharp;

namespace DotVVM.Framework.Runtime.Compilation
{
    /// <summary>
    /// Gets the Dothtml view and compiles it into a function.
    /// </summary>
    public interface IViewCompiler
    {

        /// <summary>
        /// Compiles the view and returns a function that can be invoked repeatedly. The function builds full control tree and activates the page.
        /// </summary>
        IControlBuilder CompileView(IReader reader, string fileName, string assemblyName, string namespaceName, string className);

        /// <summary>
        /// Compiles the view to a syntax tree and adds it to the compilation
        /// </summary>
        CSharpCompilation CompileView(IReader reader, string fileName, CSharpCompilation compilation, string namespaceName, string className);

        CSharpCompilation CreateCompilation(string assemblyName);
    }
}
