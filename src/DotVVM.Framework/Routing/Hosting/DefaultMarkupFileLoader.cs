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
        public string GetMarkupFileVirtualPath(DotvvmRequestContext context)
        {
            // get file name
            var fileName = context.Route != null ? context.Route.VirtualPath : context.OwinContext.Request.Uri.LocalPath;
            if (!fileName.EndsWith(MarkupFile.ViewFileExtension, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("The view must be a file with the .dothtml extension!");     // TODO: exception handling
            }

            return fileName;
        }

        /// <summary>
        /// Gets the markup file for the specified virtual path.
        /// </summary>
        public MarkupFile GetMarkup(DotvvmConfiguration configuration, string virtualPath)
        {
            // check that we are not outside application directory
            var fullPath = Path.Combine(configuration.ApplicationPhysicalPath, virtualPath);
            fullPath = Path.GetFullPath(fullPath);
            if (!fullPath.StartsWith(configuration.ApplicationPhysicalPath, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("The view cannot be located outside the website directory!");     // TODO: exception handling
            }

            // load the file
            return new MarkupFile(virtualPath, fullPath);
        }
    }
}