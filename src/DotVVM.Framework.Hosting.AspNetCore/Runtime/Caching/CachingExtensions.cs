using DotVVM.Framework.Runtime.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace DotVVM.Framework.Hosting.AspNetCore.Runtime.Caching
{
    internal static class CachingExtensions
    {
        internal static CacheItemPriority ConvertToCacheItemPriority(this DotvvmCacheItemPriority priority)
        {
            switch (priority)
            {
                case DotvvmCacheItemPriority.NeverRemove:
                    return CacheItemPriority.NeverRemove;

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
