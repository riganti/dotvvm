using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Hosting
{
    public class AggregateMarkupFileLoader : IMarkupFileLoader
    {
        public List<IMarkupFileLoader> Loaders { get; private set; } = new List<IMarkupFileLoader>();

        public AggregateMarkupFileLoader()
        {
            Loaders.Add(new DefaultMarkupFileLoader());
            Loaders.Add(new EmbeddedMarkupFileLoader());
        }

        /// <summary>
        /// Gets the markup file for the specified virtual path.
        /// </summary>
        public MarkupFile? GetMarkup(DotvvmConfiguration configuration, string virtualPath)
        {
            for (int i = 0; i < Loaders.Count; i++)
            {
                var result = Loaders[i].GetMarkup(configuration, virtualPath);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the markup file virtual path from the current request URL.
        /// </summary>
        public string GetMarkupFileVirtualPath(IDotvvmRequestContext context)
        {
            return context.Route!.VirtualPath;
        }
    }
}
