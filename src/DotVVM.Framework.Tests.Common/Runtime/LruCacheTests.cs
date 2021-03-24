using System;
using System.Threading;
using DotVVM.Framework.Runtime.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.Runtime
{
    [TestClass]
    public class LruCacheTests
    {
        [TestMethod]
        public void CreatesNew()
        {
            var dict = new SimpleLruDictionary<object, object>(100, TimeSpan.FromMilliseconds(100));
            var a = dict.GetOrCreate("a", _ => 1);
            Assert.AreEqual(1, a);
            var b = dict.GetOrCreate("b", _ => 2);
            Assert.AreEqual(2, b);
            var c = dict.GetOrCreate("c", _ => "value");
            Assert.AreEqual("value", c);
        }

        [TestMethod]
        public void KeepsOldValue()
        {
            var dict = new SimpleLruDictionary<object, object>(100, TimeSpan.FromMilliseconds(100));
            var a = dict.GetOrCreate("a", _ => 1);
            Assert.AreEqual(1, a);
            var b = dict.GetOrCreate("b", _ => 2);
            Assert.AreEqual(2, b);
            var c = dict.GetOrCreate("a", _ => "value");
            Assert.AreEqual(1, c);
        }

        // note that the checks that the table actually drops some items must count with the fact the dictionary
        // remembers everything in WeakReferences
        // the following code may look quite innocent, but it was not all that easy to craft is so that
        // JIT (in debug mode) does not store the created values somewhere and that they may be cleared by GC
        // it would not surprise me if it broke on diffent runtime than what I tested. In such case, feel free to ignore those tests

        // also, if you'd like to tests something more, I strongly suggest writing a new tests and not changing **anything** in those

        [TestMethod]
        public void ClearsWhenLarge()
        {
            var dict = new SimpleLruDictionary<object, object>(10, TimeSpan.FromMilliseconds(100));
            for (int i = 0; i < 25; i++)
                dict.GetOrCreate(i, _ => i);
            GC.Collect(2, GCCollectionMode.Forced);
            Assert.IsTrue(dict.TryGetValue(24, out _));
            Assert.IsFalse(dict.TryGetValue(1, out _));
        }

        [TestMethod]
        public void ClearsOnlyUnused()
        {
            var dict = new SimpleLruDictionary<object, object>(10, TimeSpan.FromMilliseconds(100));
            for (int i = 0; i < 25; i++)
            {
                dict.GetOrCreate(i, _ => i);
                dict.TryGetValue(0, out _);
            }
            GC.Collect(2, GCCollectionMode.Forced);
            Assert.IsFalse(dict.TryGetValue(1, out _));
            Assert.IsTrue(dict.TryGetValue(0, out _));
        }

        [TestMethod]
        public void ClearsOnTimeout()
        {
            var dict = new SimpleLruDictionary<object, object>(10, TimeSpan.FromMilliseconds(1));
            for (int i = 0; i < 20; i++)
            {
                dict.GetOrCreate(i, _ => new object());
            }
            Thread.Sleep(30); // let's give the timer some margin so this test does not fail randomly
            GC.Collect(2, GCCollectionMode.Forced);
            Assert.IsFalse(dict.TryGetValue(1, out _));
        }

        [TestMethod]
        public void IsCollectible()
        {
            // because of the timer, there might a bug that prevents extinction from the managed heap by GC

            WeakReference<SimpleLruDictionary<object, object>> create() =>
                new WeakReference<SimpleLruDictionary<object, object>>(new SimpleLruDictionary<object, object>(10, TimeSpan.FromMilliseconds(32)));
            bool hasValue(WeakReference<SimpleLruDictionary<object, object>> wr) => wr.TryGetTarget(out _);
            var dict = create();
            var counter = 0;
            while (hasValue(dict))
            {
                Thread.Sleep(32);
                GC.Collect(2, GCCollectionMode.Forced);
                Thread.Sleep(32);
                Assert.IsTrue(counter < 30); // wut, no Assert.LessThan?
                counter++;
            }
        }
    }
}
