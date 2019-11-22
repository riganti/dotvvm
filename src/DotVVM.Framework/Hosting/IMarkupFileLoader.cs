#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Hosting
{
    public interface IMarkupFileLoader
    {

        /// <summary>
        /// Gets the markup file from the current request URL.
        /// </summary>
        string GetMarkupFileVirtualPath(IDotvvmRequestContext context);

        /// <summary>
        /// Gets the markup file for the specified virtual path. Returns null when the file can not be loaded
        /// </summary>
        MarkupFile? GetMarkup(DotvvmConfiguration configuration, string virtualPath);

    }
}
