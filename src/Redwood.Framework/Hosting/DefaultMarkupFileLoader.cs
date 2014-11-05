using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Redwood.Framework.Hosting
{
    public class DefaultMarkupFileLoader : IMarkupFileLoader
    {

        /// <summary>
        /// Loads the markup.
        /// </summary>
        public MarkupFile GetMarkup(RedwoodRequestContext context)
        {
            // get file name
            var fileName = context.Route != null ? context.Route.VirtualPath : context.OwinContext.Request.Uri.LocalPath;
            if (!fileName.EndsWith(MarkupFile.ViewFileExtension, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("The view must be a file with the .rwhtml extension!");     // TODO: exception handling
            }

            // check that we are not outside application directory
            var fullPath = Path.Combine(context.Configuration.ApplicationPhysicalPath, fileName);
            fullPath = Path.GetFullPath(fullPath);
            if (!fullPath.StartsWith(context.Configuration.ApplicationPhysicalPath, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("The view cannot be located outside the website directory!");
            }

            // load the file
            return new MarkupFile()
            {
                ContentsReader = new FileReader(fullPath),
                FileName = fileName,
                FullPath = fullPath
            };
        }
    }
}