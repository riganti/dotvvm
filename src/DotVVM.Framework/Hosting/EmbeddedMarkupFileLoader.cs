using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Hosting
{
    public class EmbeddedMarkupFileLoader : IMarkupFileLoader
    {
        /// <summary>
        /// Gets the markup file for the specified virtual path.
        /// </summary>
        public MarkupFile GetMarkup(DotvvmConfiguration configuration, string virtualPath)
        {
            if (virtualPath.IndexOf('.') == -1 || virtualPath.IndexOf('.') == 0)
            {
                return null;
            }

            string assemblyName = virtualPath.Substring(0, virtualPath.IndexOf("."));

            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(new AssemblyName(assemblyName));
            }

            //no such assembly found
            catch (FileLoadException)
            {
                return null;
            }

            //no such resource found
            if( assembly.GetManifestResourceInfo(virtualPath) == null)
            { 
                return null;
            }

            //load the file
            Stream stream = assembly.GetManifestResourceStream(virtualPath);
            StreamReader sr = new StreamReader(stream);
            return new MarkupFile(virtualPath, virtualPath, sr.ReadToEnd());
        }

        /// <summary>
        /// Gets the markup file virtual path from the current request URL.
        /// </summary>
        public string GetMarkupFileVirtualPath(IDotvvmRequestContext context)
        {
            return context.Route.VirtualPath;
        }
    }
}

