using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Controls;
using Redwood.Framework.Parser;

namespace Redwood.Framework.Runtime
{
    /// <summary>
    /// Gets the RwHtml view and compiles it into a function.
    /// </summary>
    public interface IViewCompiler
    {

        /// <summary>
        /// Compiles the view and returns a function that can be invoked repeatedly. The function builds full control tree and activates the page.
        /// </summary>
        IControlBuilder CompileView(IReader reader, string fileName, string assemblyName, string namespaceName, string className);

    }
}
