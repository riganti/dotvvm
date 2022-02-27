using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Hosting;

public class AggregateMarkupFileLoaderOptions
{
    public List<Type> LoaderTypes { get; }

    public AggregateMarkupFileLoaderOptions()
    {
        LoaderTypes = new()
        {
            // the EmbeddedMarkupFileLoader must be registered before DefaultMarkupFileLoader (which gets wrapped by HotReloadMarkupFileLoader)
            typeof(EmbeddedMarkupFileLoader),
            typeof(DefaultMarkupFileLoader)
        };
    }
}
