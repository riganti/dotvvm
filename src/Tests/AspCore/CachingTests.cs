using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting.AspNetCore.Runtime.Caching;
using DotVVM.Framework.Runtime.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.AspCore
{
    [TestClass]
    public class AspCoreCachingTests
    {
        [TestMethod]
        public void NullableFactoryMethod()
        {
            var config = GetConfig();

            var cache = config.ServiceProvider.GetRequiredService<IDotvvmCacheAdapter>();
            Assert.IsNotNull(cache);

            var key = new object();
            var a = cache.GetOrAdd<object, object>(key, null);
            Assert.IsNull(a);
        }

        private static DotvvmConfiguration GetConfig()
        {
            var config = DotvvmConfiguration.CreateDefault(services => {
                services.AddMemoryCache();
                services.TryAddSingleton<IDotvvmCacheAdapter, AspNetCoreDotvvmCacheAdapter>();
            });
            return config;
        }

        [TestMethod]
        public void FactoryMethodReturnsNullForFirstTime()
        {
            var config = GetConfig();

            var cache = config.ServiceProvider.GetRequiredService<IDotvvmCacheAdapter>();
            Assert.IsNotNull(cache);

            var key = new object();
            var a = cache.GetOrAdd<object, object>(key, k => null);
            Assert.IsNull(a);

            var cachedObject = new object();
            a = cache.GetOrAdd(key, k => new DotvvmCachedItem<object>(cachedObject));
            Assert.IsNotNull(a);
            Assert.AreEqual(a, cachedObject);
        }

        [TestMethod]
        public void FactoryMethodReturnsValueNullForFirstTime()
        {
            var config = GetConfig();

            var cache = config.ServiceProvider.GetRequiredService<IDotvvmCacheAdapter>();
            Assert.IsNotNull(cache);

            var key = new object();
            var a = cache.GetOrAdd(key, k => new DotvvmCachedItem<object>(null));
            Assert.IsNull(a);

            var cachedObject = new object();
            a = cache.GetOrAdd(key, k => new DotvvmCachedItem<object>(cachedObject));
            Assert.IsNotNull(a);
            Assert.AreEqual(a, cachedObject);
        }

        [TestMethod]
        public void AddGetRemove()
        {
            var config = GetConfig();
            var cache = config.ServiceProvider.GetRequiredService<IDotvvmCacheAdapter>();
            Assert.IsNotNull(cache);

            var key = new object();
            var key2 = new object();
            var value = "value1";
            var value2 = "value2";

            DotvvmCachedItem<string> factoryFunc(object k)
            {
                if (key == k)
                {
                    return new DotvvmCachedItem<string>(value);
                }
                else
                {
                    return new DotvvmCachedItem<string>(value2);
                }
            }

            // ADD

            var a = cache.GetOrAdd(key, factoryFunc);
            var b = cache.GetOrAdd(key2, factoryFunc);

            Assert.AreEqual(a, value);
            Assert.AreEqual(b, value2);

            // GET
            a = cache.GetOrAdd<object, string>(key, null);
            Assert.AreEqual(a, value);

            b = cache.GetOrAdd<object, string>(key2, null);
            Assert.AreEqual(b, value2);

            // REMOVE
            cache.Remove(key2);
            a = cache.GetOrAdd<object, string>(key, null);
            Assert.AreEqual(a, value);

            b = cache.GetOrAdd<object, string>(key2, null);
            Assert.IsNull(b);

            cache.Remove(key);
            a = cache.GetOrAdd<object, string>(key, null);
            Assert.IsNull(a);
        }
    }
}
