using DotVVM.Framework.Tools.SeleniumGenerator.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DotVVM.Testing.SeleniumGenerator.Tests
{
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void DictionaryExtensions_CheckDictionaryUnion()
        {
            // initialize
            var firstDict = new Dictionary<int, string>
            {
                { 1, "a" },
                { 2, "b" }
            };

            var secondDict = new Dictionary<int, string>{
                { 3, "c" },
                { 4, "d" }
            };

            // do
            var union = firstDict.Union(secondDict);

            // assert
            Assert.AreEqual(union.Count, 4);
            Assert.AreEqual(union[1], "a");
            Assert.AreEqual(union[2], "b");
            Assert.AreEqual(union[3], "c");
            Assert.AreEqual(union[4], "d");
        }
    }
}
