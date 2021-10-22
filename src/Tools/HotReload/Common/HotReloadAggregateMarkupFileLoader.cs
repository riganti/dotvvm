using System;
using DotVVM.Framework.Hosting;

namespace DotVVM.HotReload
{
    public class HotReloadAggregateMarkupFileLoader : AggregateMarkupFileLoader
    {

        public HotReloadAggregateMarkupFileLoader(IMarkupFileChangeNotifier notifier)
        {
            var index = Loaders.FindIndex(l => l is DefaultMarkupFileLoader);
            if (index < 0)
            {
                throw new InvalidOperationException("DotVVM Hot reload could not be initialized - the DefaultMarkupLoader was not found in the AggregateMarkupFileLoader Loaders collection.");
            }
            var defaultLoader = (DefaultMarkupFileLoader)Loaders[index];
            Loaders[index] = new HotReloadMarkupFileLoader(defaultLoader, notifier);
        }

    }
}
