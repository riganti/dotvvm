using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Hosting
{
    public interface IMarkupFileLoader
    {

        /// <summary>
        /// Gets the markup file from the current request URL.
        /// </summary>
        MarkupFile GetMarkup(RedwoodRequestContext context);

        /// <summary>
        /// Gets the markup file for the specified virtual path.
        /// </summary>
        MarkupFile GetMarkup(RedwoodRequestContext context, string virtualPath);

    }
}