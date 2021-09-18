using DotVVM.Framework.Hosting;

namespace DotVVM.Diagnostics.ViewHotReload
{
    public class HotReloadAggregateMarkupFileLoader : AggregateMarkupFileLoader
    {

        public HotReloadAggregateMarkupFileLoader(IMarkupFileChangeNotifier notifier)
        {
            var index = Loaders.FindIndex(l => l is DefaultMarkupFileLoader);
            var defaultLoader = (DefaultMarkupFileLoader)Loaders[index];
            Loaders[index] = new HotReloadMarkupFileLoader(defaultLoader, notifier);
        }

    }
}
