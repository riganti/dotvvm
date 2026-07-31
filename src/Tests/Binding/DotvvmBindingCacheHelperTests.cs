#nullable enable

using System;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Runtime.Caching;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Binding
{
    [TestClass]
    public class DotvvmBindingCacheHelperTests
    {
        private BindingCompilationService service = null!;
        private DotvvmBindingCacheHelper helper = null!;

        [TestInitialize]
        public void Initialize()
        {
            service = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetRequiredService<BindingCompilationService>();
            helper = new DotvvmBindingCacheHelper(new SimpleDictionaryCacheAdapter(), service);
        }

        [TestMethod]
        public void Constructor_RejectsNullCache()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DotvvmBindingCacheHelper(null!, service));
        }

        [TestMethod]
        public void CreateCachedBinding_CachesByBindingTypeIdentifierAndStructuralKeys()
        {
            var factoryCalls = 0;

            var first = helper.CreateCachedBinding("binding", new object?[] { "key", 42 }, () => {
                factoryCalls++;
                return new TestBinding();
            });
            var same = helper.CreateCachedBinding("binding", new object?[] { new string("key".ToCharArray()), 42 }, () => {
                factoryCalls++;
                return new TestBinding();
            });
            var differentIdentifier = helper.CreateCachedBinding("other", new object?[] { "key", 42 }, () => new TestBinding());
            var differentKeyOrder = helper.CreateCachedBinding("binding", new object?[] { 42, "key" }, () => new TestBinding());
            var differentBindingType = helper.CreateCachedBinding<OtherTestBinding>("binding", new object?[] { "key", 42 }, () => new OtherTestBinding());

            Assert.AreSame(first, same);
            Assert.AreEqual(1, factoryCalls);
            Assert.AreNotSame(first, differentIdentifier);
            Assert.AreNotSame(first, differentKeyOrder);
            Assert.AreNotSame<object>(first, differentBindingType);
        }

        [TestMethod]
        public void CreateCachedBinding_AllowsNullOverriddenEqualsAndExplicitReferenceEqualityKeys()
        {
            var referenceKey = new object();

            var binding = helper.CreateCachedBinding(
                "binding",
                new object?[] { null, new EquatableKey(1), new Tuple<object>(referenceKey) },
                () => new TestBinding());

            Assert.IsNotNull(binding);
        }

        [TestMethod]
        public void CreateCachedBinding_RejectsKeyWithoutObjectEqualsOverride()
        {
            var exception = Assert.ThrowsException<Exception>(() =>
                helper.CreateCachedBinding("binding", new object?[] { new ReferenceKey() }, () => new TestBinding()));

            StringAssert.Contains(exception.Message, typeof(ReferenceKey).FullName!);
            StringAssert.Contains(exception.Message, "Object.Equals");
        }

        [TestMethod]
        public void CreateCachedBinding_StoresHighPriorityItem()
        {
            var cache = new RecordingCacheAdapter();
            var helper = new DotvvmBindingCacheHelper(cache, service);

            helper.CreateCachedBinding("binding", Array.Empty<object?>(), () => new TestBinding());

            Assert.AreEqual(DotvvmCacheItemPriority.High, cache.LastPriority);
        }

        [TestMethod]
        public void BindingFactoryMethods_CacheEquivalentRequestsAndSeparateBindingKinds()
        {
            var context = DataContextStack.Create(typeof(CacheViewModel));

            var value = helper.CreateValueBinding<bool>("Flag", context);
            var sameValue = helper.CreateValueBinding<bool>("Flag", context);
            var untypedValue = helper.CreateValueBinding("Flag", context);
            var resource = helper.CreateResourceBinding<bool>("Flag", context);
            var sameResource = helper.CreateResourceBinding<bool>("Flag", context);
            var command = helper.CreateCommand<Action>("() => Flag = !Flag", context);
            var sameCommand = helper.CreateCommand<Action>("() => Flag = !Flag", context);
            var staticCommand = helper.CreateStaticCommand<Action>("() => Flag = !Flag", context);
            var sameStaticCommand = helper.CreateStaticCommand<Action>("() => Flag = !Flag", context);

            Assert.AreSame(value, sameValue);
            Assert.AreNotSame(value, untypedValue);
            Assert.AreSame(resource, sameResource);
            Assert.AreSame(command, sameCommand);
            Assert.AreSame(staticCommand, sameStaticCommand);
            Assert.AreNotSame<object>(command, staticCommand);
        }

        private class TestBinding : IBinding
        {
            public DataContextStack? DataContext => null;
            public BindingResolverCollection? GetAdditionalResolvers() => null;
            public object? GetProperty(Type type, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException) => null;
        }

        private sealed class OtherTestBinding : TestBinding { }

        private sealed record EquatableKey(int Value);
        private sealed class ReferenceKey { }

        private sealed class CacheViewModel
        {
            public bool Flag { get; set; }
        }

        private sealed class RecordingCacheAdapter : IDotvvmCacheAdapter
        {
            public DotvvmCacheItemPriority? LastPriority { get; private set; }

            public T GetOrAdd<TKey, T>(TKey key, Func<TKey, DotvvmCachedItem<T>> factoryFunc) where TKey : notnull
            {
                var item = factoryFunc(key);
                LastPriority = item.Priority;
                return item.Value;
            }

            public T? Get<T>(object key) => default;
            public object? Remove(object key) => null;
        }
    }
}
