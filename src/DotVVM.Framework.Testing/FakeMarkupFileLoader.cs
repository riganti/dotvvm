using System.Collections.Generic;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using System.Collections.Concurrent;

namespace DotVVM.Framework.Testing
{
    public class FakeMarkupFileLoader : IMarkupFileLoader
    {
        public readonly ConcurrentDictionary<string, string> MarkupFiles;

        public FakeMarkupFileLoader(Dictionary<string, string>? markupFiles = null)
        {
            this.MarkupFiles = new ConcurrentDictionary<string, string>(markupFiles ?? new Dictionary<string, string>());
        }

        public MarkupFile GetMarkup(DotvvmConfiguration configuration, string virtualPath)
        {
            return new MarkupFile(virtualPath, virtualPath, MarkupFiles[virtualPath]);
        }

        public string GetMarkupFileVirtualPath(Hosting.IDotvvmRequestContext context)
        {
            return context.Route!.VirtualPath;
        }
    }
}
