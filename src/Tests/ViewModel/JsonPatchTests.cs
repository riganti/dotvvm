using System;
using System.Linq;
using System.Text.Json.Nodes;
using DotVVM.Framework.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class JsonPatchTests
    {

        [DataTestMethod]
        [DataRow(null, """["a", "b"]""")]
        [DataRow("""["a", "d"]""", """["a", "d"]""")]
        [DataRow("""["a", "b", "c"]""", """["a", "b", "c"]""")]
        [DataRow("""["a"]""", """["a"]""")]
        public void JsonPatch_Arrays_Primitive(string diffJson, string resultJson)
        {
            CheckCore(WrapArray("""["a", "b"]"""), WrapArray(diffJson), WrapArray(resultJson));
        }

        [DataTestMethod]
        [DataRow("""{ }""", """{ "a": 1, "b": "a" }""")]
        [DataRow("""{ "a": 2 }""", """{ "a": 2, "b": "a" }""")]
        [DataRow("""{ "a": 2, "c": null }""", """{ "a": 2, "b": "a", "c": null }""")]
        [DataRow("""{ "a": 2 }""", """{ "a": 2, "b": "a" }""")]
        public void JsonPatch_Objects(string diffJson, string resultJson)
        {
            CheckCore("""{ "a": 1, "b": "a" }""", diffJson, resultJson);
        }

        [DataTestMethod]
        [DataRow("""{ }""", """{ "a": 1, "bparent": { "b": "a", "c": 1 } }""")]
        [DataRow("""{ "a": 2, "b": "a" }""", """{ "a": 2, "b": "a", "bparent": { "b": "a", "c": 1 } }""")]
        [DataRow("""{ "bparent": { "c": 2 } }""", """{ "a": 1, "bparent": { "b": "a", "c": 2 } }""")]
        [DataRow("""{ "bparent": { "c": 2, "d": 3 } }""", """{ "a": 1, "bparent": { "b": "a", "c": 2, "d": 3 } }""")]
        [DataRow("""{ "bparent": { "b": "b" } }""", """{ "a": 1, "bparent": { "b": "b", "c": 1 } }""")]
        public void JsonPatch_Objects_Nested(string diffJson, string resultJson)
        {
            CheckCore("""{ "a": 1, "bparent": { "b": "a", "c": 1 } }""", diffJson, resultJson);
        }

        [DataTestMethod]
        [DataRow(null, """[{ "a": 1 }, { "a": 2 }]""")]
        [DataRow("""[{ "a": 3 }, {}]""", """[{ "a": 3 }, { "a": 2 }]""")]
        [DataRow("""[{}, {}, { "a": 3 }]""", """[{ "a": 1 }, { "a": 2 }, { "a": 3 }]""")]
        [DataRow("""[{ "a": 2 }]""", """[{ "a": 2 }]""")]
        [DataRow("""[{}]""", """[{ "a": 1 }]""")]
        public void JsonPatch_ObjectArray(string diffJson, string resultJson)
        {
            CheckCore(WrapArray("""[{ "a": 1 }, { "a": 2 }]"""), WrapArray(diffJson), WrapArray(resultJson));
        }

        private void CheckCore(string originalJson, string diffJson, string resultJson)
        {
            var original = JsonNode.Parse(originalJson)!.AsObject();
            var modified = original.DeepClone().AsObject();
            var diff = JsonNode.Parse(diffJson)!.AsObject();
            var result = JsonNode.Parse(resultJson)!.AsObject();
            JsonUtils.Patch(modified, diff);
            Assert.IsTrue(JsonNode.DeepEquals(modified, result), $"Expected: {result}, modified: {modified}");
            var newDiff = JsonUtils.Diff(original, modified);
            Assert.IsTrue(JsonNode.DeepEquals(diff, newDiff), $"Original diff: {diff}, computed diff: {newDiff}");
        }

        private string WrapArray(string a)
        {
            if (a == null)
            {
                return "{}";
            }

            return "{\"array\": " + a + "}";
        }
    }
}
