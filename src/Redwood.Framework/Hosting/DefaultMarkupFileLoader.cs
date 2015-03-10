using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redwood.Framework.Configuration;

namespace Redwood.Framework.Hosting
{
    public class DefaultMarkupFileLoader : IMarkupFileLoader
    {


        /// <summary>
        /// Gets the markup file from the current request URL.
        /// </summary>
        public MarkupFile GetMarkup(RedwoodRequestContext context)
        {
            // get file name
            var fileName = context.Route != null ? context.Route.VirtualPath : context.OwinContext.Request.Uri.LocalPath;
            if (!fileName.EndsWith(MarkupFile.ViewFileExtension, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("The view must be a file with the .rwhtml extension!");     // TODO: exception handling
            }

            return GetMarkupCore(context.Configuration, fileName);
        }

        /// <summary>
        /// Gets the markup file for the specified virtual path.
        /// </summary>
        public MarkupFile GetMarkup(RedwoodConfiguration configuration, string virtualPath)
        {
            return GetMarkupCore(configuration, virtualPath);
        }
         
        private static MarkupFile GetMarkupCore(RedwoodConfiguration configuration, string fileName)
        {
            // check that we are not outside application directory
            var fullPath = Path.Combine(configuration.ApplicationPhysicalPath, fileName);
            fullPath = Path.GetFullPath(fullPath);
            if (!fullPath.StartsWith(configuration.ApplicationPhysicalPath, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("The view cannot be located outside the website directory!");     // TODO: exception handling
            }

            // load the file
            return new MarkupFile(fileName, fullPath);
        }
    }
}