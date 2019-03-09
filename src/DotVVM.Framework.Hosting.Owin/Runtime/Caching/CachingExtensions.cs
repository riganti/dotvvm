using System.Web.Caching;
using DotVVM.Framework.Runtime.Caching;

namespace DotVVM.Framework.Hosting.Owin.Runtime.Caching
{
    internal static class CachingExtensions
    {
        internal static CacheItemPriority ConvertToCacheItemPriority(this DotvvmCacheItemPriority priority)
        {
            switch (priority)
            {
                case DotvvmCacheItemPriority.NeverRemove:
                    return CacheItemPriority.NotRemovable;

                case DotvvmCacheItemPriority.High:
                    return CacheItemPriority.High;

                case DotvvmCacheItemPriority.Normal:
                    return CacheItemPriority.Normal;

                default:
                    return CacheItemPriority.Low;
            }
        }
    }
}
