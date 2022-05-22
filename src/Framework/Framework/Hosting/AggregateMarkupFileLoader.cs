using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotVVM.Framework.Hosting
{
    public class AggregateMarkupFileLoader : IMarkupFileLoader
    {
        private readonly List<IMarkupFileLoader> loaders;

        public AggregateMarkupFileLoader(IOptions<AggregateMarkupFileLoaderOptions> options, IServiceProvider serviceProvider)
        {
            loaders = options.Value.LoaderTypes
                .Select(p => (IMarkupFileLoader)serviceProvider.GetRequiredService(p))
                .ToList();
        }

        /// <summary>
        /// Gets the markup file for the specified virtual path.
        /// </summary>
        public MarkupFile? GetMarkup(DotvvmConfiguration configuration, string virtualPath)
        {
            for (var i = 0; i < loaders.Count; i++)
            {
                var result = loaders[i].GetMarkup(configuration, virtualPath);
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
