using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Runtime.Caching;

namespace DotVVM.Framework.Binding
{
    public class DotvvmBindingCacheHelper
    {
        private readonly IDotvvmCacheAdapter cache;
        private readonly BindingCompilationService compilationService;

        public DotvvmBindingCacheHelper(IDotvvmCacheAdapter cache, BindingCompilationService compilationService)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.compilationService = compilationService;
        }

        public T CreateCachedBinding<T>(string identifier, object?[] keys, Func<T> factory) where T: IBinding
        {
            return this.cache.GetOrAdd(new CacheKey(typeof(T), identifier, keys), _ => {
                foreach (var k in keys)
                    CheckEqualsImplementation(k);
                return new DotvvmCachedItem<T>(factory(), DotvvmCacheItemPriority.High);
            });
        }

        public T CreateCachedBinding<T>(string identifier, object[] keys, object[] properties) where T: IBinding
        {
            return CreateCachedBinding<T>(identifier, keys, () => (T)BindingFactory.CreateBinding(this.compilationService, typeof(T), properties));
        }

        internal static void CheckEqualsImplementation(object? k)
        {
            // whitelist for some common singletons
            if (k is null || k is DotvvmProperty) return;

            var t = k.GetType();
            var eqMethod = t.GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance, null, new [] { typeof(object) }, null);
            if (eqMethod?.GetBaseDefinition().DeclaringType != typeof(object) || eqMethod.DeclaringType == typeof(object))
            {
                throw new Exception($"Instance of type {t} cannot be used as a cache key, because it does not have Object.Equals method overridden. If you really want to use referential equality (you are using a singleton as a cache key or something like that), you can wrap in Tuple<T>.");
            }
        }

        class CacheKey: IEquatable<CacheKey>
        {
            private readonly object?[] keys;
            private readonly Type type;
            private readonly string id;

            public CacheKey(Type type, string id, object?[] keys)
            {
                this.type = type ?? throw new ArgumentNullException(nameof(type));
                this.id = id ?? throw new ArgumentNullException("Cache identifier can't be null", nameof(id));
                this.keys = keys ?? throw new ArgumentNullException(nameof(keys));
            }
            public bool Equals(CacheKey? other)
            {
                if (other == null || other.keys.Length != keys.Length || other.type != type || id != other.id) return false;
                for (int i = 0; i < keys.Length; i++)
                {
                    if (!object.Equals(other.keys[i], this.keys[i]))
                        return false;
                }
                return true;
            }
            public override bool Equals(object? obj) => Equals(obj as CacheKey);
            public override int GetHashCode()
            {
                var hash = 234567643 ^ keys.Length ^ id.GetHashCode() ^ type.GetHashCode();
                foreach (var k in keys)
                {
                    hash *= 17;
                    hash += k?.GetHashCode() ?? 0;
                }
                return hash;
            }
        }
    }
}
