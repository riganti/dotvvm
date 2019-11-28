using System;
using System.Linq;
using DotVVM.Framework.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.Tests.Common.ViewModel
{
    [TestClass]
    public class JsonPatchTests
    {

        [DataTestMethod]
        [DataRow(null, "['a', 'b']")]
        [DataRow("['a', 'd']", "['a', 'd']")]
        [DataRow("['a', 'b', 'c']", "['a', 'b', 'c']")]
        [DataRow("['a']", "['a']")]
        public void JsonPatch_Arrays_Primitive(string modifiedJson, string resultJson)
        {
            var original = JObject.Parse(WrapArray("['a', 'b']"));
            var modified = JObject.Parse(WrapArray(modifiedJson));
            var result = JObject.Parse(WrapArray(resultJson));
            JsonUtils.Patch(original, modified);
            Assert.IsTrue(JToken.DeepEquals(original, result));
        }

        [DataTestMethod]
        [DataRow("{ }", "{ 'a': 1, 'b': 'a' }")]
        [DataRow("{ 'a': 2 }", "{ 'a': 2, 'b': 'a' }")]
        [DataRow("{ 'a': 2, c: null }", "{ 'a': 2, 'b': 'a', 'c': null }")]
        [DataRow("{ 'a': 2 }", "{ 'a': 2, 'b': 'a' }")]
        public void JsonPatch_Objects(string modifiedJson, string resultJson)
        {
            var original = JObject.Parse("{ 'a': 1, 'b': 'a' }");
            var modified = JObject.Parse(modifiedJson);
            var result = JObject.Parse(resultJson);
            JsonUtils.Patch(original, modified);
            Assert.IsTrue(JToken.DeepEquals(original, result));
        }

        [DataTestMethod]
        [DataRow("{ }", "{ 'a': 1, 'bparent': { 'b': 'a', 'c': 1 } }")]
        [DataRow("{ 'a': 2, 'b': 'a' }", "{ 'a': 2, 'b': 'a', 'bparent': { 'b': 'a', 'c': 1 } }")]
        [DataRow("{ 'bparent': { 'c': 2 } }", "{ 'a': 1, 'bparent': { 'b': 'a', 'c': 2 } }")]
        [DataRow("{ 'bparent': { 'c': 2, 'd': 3 } }", "{ 'a': 1, 'bparent': { 'b': 'a', 'c': 2, 'd': 3 } }")]
        [DataRow("{ 'bparent': { 'b': 'b' } }", "{ 'a': 1, 'bparent': { 'b': 'b', 'c': 1 } }")]
        public void JsonPatch_Objects_Nested(string modifiedJson, string resultJson)
        {
            var original = JObject.Parse("{ 'a': 1, 'bparent': { 'b': 'a', 'c': 1 } }");
            var modified = JObject.Parse(modifiedJson);
            var result = JObject.Parse(resultJson);
            JsonUtils.Patch(original, modified);
            Assert.IsTrue(JToken.DeepEquals(original, result));
        }

        [DataTestMethod]
        [DataRow(null, "[{ 'a': 1 }, { 'a': 2 }]")]
        [DataRow("[{ 'a': 3 }, {}]", "[{ 'a': 3 }, { 'a': 2 }]")]
        [DataRow("[{}, {}, { 'a': 3 }]", "[{ 'a': 1 }, { 'a': 2 }, { 'a': 3 }]")]
        [DataRow("[{ 'a': 2 }]", "[{ 'a': 2 }]")]
        [DataRow("[{}]", "[{ 'a': 1 }]")]
        public void JsonPatch_ObjectArray(string modifiedJson, string resultJson)
        {
            var original = JObject.Parse(WrapArray("[{ 'a': 1 }, { 'a': 2 }]"));
            var modified = JObject.Parse(WrapArray(modifiedJson));
            var result = JObject.Parse(WrapArray(resultJson));
            JsonUtils.Patch(original, modified);
            Assert.IsTrue(JToken.DeepEquals(original, result));
        }

        private string WrapArray(string a)
        {
            if (a == null)
            {
                return "{}";
            }

            return "{'array': " + a + "}";
        }
    }
}
