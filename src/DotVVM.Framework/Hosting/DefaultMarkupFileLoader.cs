using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Hosting
{
    public class DefaultMarkupFileLoader : IMarkupFileLoader
    {


        /// <summary>
        /// Gets the markup file virtual path from the current request URL.
        /// </summary>
        public string GetMarkupFileVirtualPath(IDotvvmRequestContext context)
        {
            return context.Route.VirtualPath;
        }

        /// <summary>
        /// Gets the markup file for the specified virtual path.
        /// </summary>
        public MarkupFile GetMarkup(DotvvmConfiguration configuration, string virtualPath)
        {
            // check that we are not outside application directory
            var fullPath = Path.Combine(configuration.ApplicationPhysicalPath, virtualPath);
            fullPath = Path.GetFullPath(fullPath);
            if (!fullPath.Replace('\\', '/').StartsWith(configuration.ApplicationPhysicalPath.Replace('\\', '/'), StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("The view cannot be located outside the website directory!");     // TODO: exception handling
            }

            // load the file
            return new MarkupFile(virtualPath, fullPath);
        }
    }
}