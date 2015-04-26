using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Configuration;

namespace Redwood.Framework.Hosting
{
    public interface IMarkupFileLoader
    {

        /// <summary>
        /// Gets the markup file from the current request URL.
        /// </summary>
        string GetMarkupFileVirtualPath(RedwoodRequestContext context);

        /// <summary>
        /// Gets the markup file for the specified virtual path.
        /// </summary>
        MarkupFile GetMarkup(RedwoodConfiguration configuration, string virtualPath);

    }
}