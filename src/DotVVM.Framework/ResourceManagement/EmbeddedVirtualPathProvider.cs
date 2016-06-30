using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    public class EmbeddedVirtualPathProvider : VirtualPathProvider
    {
        public EmbeddedVirtualPathProvider()
        {

        }

        private bool IsEmbeddedResourcePathDotvvm(string virtualPath)
        {
            var checkPath = VirtualPathUtility.ToAppRelative(virtualPath);
            return checkPath.StartsWith("~/DotVVM.Framework.dll/", StringComparison.InvariantCultureIgnoreCase);
        }

        private bool IsEmbeddedResourcePathBootstrap(string virtualPath)
        {
            var checkPath = VirtualPathUtility.ToAppRelative(virtualPath);
            return checkPath.StartsWith("~/DotVVM.Framework.Controls.Bootstrap.dll/", StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool FileExists(string virtualPath)
        {
            return IsEmbeddedResourcePathDotvvm(virtualPath) || IsEmbeddedResourcePathBootstrap(virtualPath) || base.FileExists(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            if (IsEmbeddedResourcePathDotvvm(virtualPath))
            {
                return new EmbeddedVirtualFile(virtualPath);
            }
            if (IsEmbeddedResourcePathBootstrap(virtualPath))
            {
                return new EmbeddedVirtualFile(virtualPath);
            }
            else
            {
                return base.GetFile(virtualPath);
            }
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies,
            DateTime utcStart)
        {

            if (IsEmbeddedResourcePathDotvvm(virtualPath) || IsEmbeddedResourcePathBootstrap(virtualPath))
            {
                return null;
            }
            if (virtualPathDependencies.Cast<string>().ToList().Any(dependency => IsEmbeddedResourcePathDotvvm(dependency) || IsEmbeddedResourcePathBootstrap(dependency)))
            {
                return null;
            }

            return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);

        }
    }
}
