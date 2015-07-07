using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser;

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

    }
}
