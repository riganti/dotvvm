using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    public class EmbeddedVirtualFile : VirtualFile
    {
        private string path;

        public EmbeddedVirtualFile(string virtualPath) : base(virtualPath)
        {
            path = VirtualPathUtility.ToAppRelative(virtualPath);
        }

        public override Stream Open()
        {
            string[] parts = path.Split('/');
            string assemblyName = parts[1];
            string resourceName = parts[2];


            assemblyName = Path.Combine(HttpRuntime.BinDirectory, assemblyName);

            System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(assemblyName);
            if (assembly != null)
            {
                Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
                return resourceStream;
            }
            return null;
        }
    }
}
