using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Routing
{
    [TestClass]
    public class UrlHelperTests
    {

        [DataTestMethod]
        [DataRow("/local", true)]
        [DataRow("~/local", true)]
        [DataRow("//local", false)]
        [DataRow("~//local", false)]
        [DataRow("https://www.google.com", false)]
        [DataRow("/https://www.google.com", true)]
        [DataRow("/\r\n/google.com", false)]
        [DataRow("/\n/google.com", false)]
        [DataRow("mailto:a", false)]
        [DataRow("/mailto:a", true)]
        [DataRow(@"\\www.google.com", false)] // Chrome replaces backslashes with forward slashes...
        [DataRow(@"\/www.google.com", false)]
        [DataRow(@"/\www.google.com", false)]
        [DataRow(@"/4aef74ba-388c-4292-9d53-98387e4f797b/reservation?LocationId=e5eed4c5-dfe9-45fd-a341-7408205d76ce&BeginDate=201909011300&Duration=2", true)]
        public void UrlHelper_IsLocalUrl(string url, bool expectedResult)
        {
            var result = UrlHelper.IsLocalUrl(url);
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void UrlHelper_BuildUrlSuffix_EnumerableStringString()
        {
            var suffix = "suffix";
            var query = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("key1", "value1"),
                new KeyValuePair<string, string>("key2", null!),
                new KeyValuePair<string, string>("key3", string.Empty),
                new KeyValuePair<string, string>("key4", "value4")
            };
            var result = UrlHelper.BuildUrlSuffix(suffix, query);
            Assert.AreEqual("suffix?key1=value1&key3&key4=value4", result);
        }

        [TestMethod]
        public void UrlHelper_BuildUrlSuffix_EnumerableStringObject()
        {
            var suffix = "suffix";
            var query = new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>("key1", "value1"),
                new KeyValuePair<string, object>("key2", null!),
                new KeyValuePair<string, object>("key3", string.Empty),
                new KeyValuePair<string, object>("key4", "value4")
            };
            var result = UrlHelper.BuildUrlSuffix(suffix, query);
            Assert.AreEqual("suffix?key1=value1&key3&key4=value4", result);
        }

        [TestMethod]
        public void UrlHelper_BuildUrlSuffix_Object()
        {
            var suffix = "suffix";
            var query = new TestUrlSuffixDescriptor();
            var result = UrlHelper.BuildUrlSuffix(suffix, query);
            Assert.AreEqual("suffix?key1=value1&key3&key4=value4", result);
        }

        private class TestUrlSuffixDescriptor
        {
            public string key1 { get; set; } = "value1";
            public object key2 { get; set; } = null;
            public object key3 { get; set; } = string.Empty;
            public string key4 { get; set; } = "value4";
        }
    }
}
