using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using System.Text.Json;
using DotVVM.Framework.Hosting;
using System.Buffers;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class JsonDiffTests
    {
        private JsonSerializerOptions serializerOptions = VisualStudioHelper.GetSerializerOptions();

        public JsonDiffTests()
        {
            DotvvmTestHelper.EnsureCompiledAssemblyCache();
        }

        [TestMethod]
        public void JsonDiff_SimpleTest()
        {
            var a = JsonNode.Parse("""{"name":"djsfsh","ahoj":45}""")!.AsObject();
            var b = JsonNode.Parse("""{"name":"djsfsh","ahoj":42}""")!.AsObject();
            var diff = Utf8JsonDiff(a, b);
            JsonUtils.Patch(a, diff);
            Assert.IsTrue(JsonNode.DeepEquals(a, b));
            Assert.AreEqual("""{"ahoj":42}""", diff.ToJsonString());
        }

        [TestMethod]
        [Ignore("DotvvmConfiguration deserialization is not currently implemented")]
        public void JsonDiff_Configuration_AddingResources()
        {
            var config = ApplyPatches(
                CreateDiff(c => c.Resources.Register("resource-1", new InlineScriptResource("alert()"))),
                CreateDiff(c => c.Resources.Register("resource-2", new InlineScriptResource("console.log()"))),
                CreateDiff(c => c.Resources.Register("resource-3", new ScriptResource(new UrlResourceLocation("http://i.dont.know/which.js")) { Dependencies = new[] { "dotvvm" } }))
                );
            Assert.IsInstanceOfType(config.Resources.FindResource("resource-1"), typeof(InlineScriptResource));
            Assert.IsInstanceOfType(config.Resources.FindResource("resource-2"), typeof(InlineScriptResource));
            Assert.IsInstanceOfType(config.Resources.FindResource("resource-3"), typeof(ScriptResource));
            Assert.AreEqual("dotvvm", (config.Resources.FindResource("resource-3") as ScriptResource).Dependencies.Single());
        }

        [TestMethod]
        [Ignore("DotvvmConfiguration deserialization is not currently implemented")]
        public void JsonDiff_Configuration_AddingRoute()
        {
            var config = ApplyPatches(
                CreateDiff(c => c.RouteTable.Add("Route1", "Path1", "View1.dothtml")),
                CreateDiff(c => c.RouteTable.Add("Route2", "Path2", "View2.dothtml")),
                CreateDiff(c => c.RouteTable.Add("Route3", "Path3/{Name}", "View3.dothtml", new { Name = "defaultname" }))
                );
            XAssert.Contains("Route1", config.RouteTable.Select(r => r.RouteName));
            XAssert.Contains("Route2", config.RouteTable.Select(r => r.RouteName));
            XAssert.Contains("Route3", config.RouteTable.Select(r => r.RouteName));
            Assert.AreEqual("View1.dothtml", config.RouteTable.Single(r => r.RouteName == "Route1").VirtualPath);
            Assert.AreEqual("defaultname", config.RouteTable.Single(r => r.RouteName == "Route3").DefaultValues["Name"]);
        }

        [TestMethod]
        public void JsonDiff_BusinessPackFilter_NoThrow()
        {
            var source = JsonNode.Parse(
@"{
    ""FieldName"": ""DateRequiredBy"",
    ""FieldDisplayName"": null,
    ""Operator"": ""LessThan"",
    ""FormatString"": null,
    ""Value"": ""2019-08-07T00:00:00"",
    ""Type"": ""FilterCondition""
}")!.AsObject();
            var target = JsonNode.Parse(
@"{
    ""FieldName"": ""Code"",
    ""FieldDisplayName"": null,
    ""Operator"": ""Equal"",
    ""FormatString"": null,
    ""Value"": ""HHK"",
    ""Type"": ""FilterCondition""
}")!.AsObject();
            var diff = Utf8JsonDiff(source, target);
            Assert.AreEqual("""{"FieldName":"Code","Operator":"Equal","Value":"HHK"}""", diff.ToJsonString());
        }

        private JsonObject CreateDiff(Action<DotvvmConfiguration> fn)
        {
            var config = DotvvmTestHelper.CreateConfiguration();
            var json0 = JsonSerializer.SerializeToNode(config, serializerOptions)!.AsObject();
            fn(config);
            var json1 = JsonSerializer.SerializeToNode(config, serializerOptions)!.AsObject();
            return Utf8JsonDiff(json0, json1);
        }

        private DotvvmConfiguration ApplyPatches(DotvvmConfiguration init, params JsonObject[] patches)
        {
            var json = JsonSerializer.SerializeToNode(init, serializerOptions)!.AsObject();
            foreach (var p in patches)
            {
                Console.WriteLine("Applying patch: " + p.ToJsonString());
                JsonUtils.Patch(json, p);
            }
            return JsonSerializer.Deserialize<DotvvmConfiguration>(json, serializerOptions);
        }

        private DotvvmConfiguration ApplyPatches(params JsonObject[] patches) => ApplyPatches(DotvvmTestHelper.CreateConfiguration(), patches);

        public static JsonObject Utf8JsonDiff(JsonObject source, JsonObject target)
        {
            var sourceDoc = JsonDocument.Parse(source.ToJsonString());
            var targetJson = StringUtils.Utf8.GetBytes(target.ToJsonString());

            var buffer = new MemoryStream();
            using (var writer = new Utf8JsonWriter(buffer))
                JsonDiffWriter.ComputeDiff(writer, sourceDoc.RootElement, targetJson);
            Console.WriteLine(StringUtils.Utf8.GetString(buffer.ToSpan()));
            return (JsonObject)JsonNode.Parse(buffer.ToSpan());
        }

        public static void ValidateDiff(JsonObject source, JsonObject target)
        {
            var diffJsonUtils = JsonUtils.Diff(source, target);
            var diffJsonDiffWriter = Utf8JsonDiff(source, target);

            Assert.AreEqual(diffJsonUtils.ToJsonString(), diffJsonDiffWriter.ToJsonString(),
                            $"JsonDiffWriter output differs from JsonUtils output.\nExpected: {diffJsonUtils.ToJsonString()}\nActual:   {diffJsonDiffWriter.ToJsonString()}");
        }

        [TestMethod]
        public void JsonDiffWriter_Empty()
        {
            var a = JsonNode.Parse("{}")!.AsObject();
            var b = JsonNode.Parse("{}")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_NullValues()
        {
            var a = JsonNode.Parse("""{"a":null}""")!.AsObject();
            var b = JsonNode.Parse("""{"a":null}""")!.AsObject();
            ValidateDiff(a, b);

            var c = JsonNode.Parse("""{"a":null,"b":1}""")!.AsObject();
            var d = JsonNode.Parse("""{"a":2,"b":1}""")!.AsObject();
            ValidateDiff(c, d);
        }

        [TestMethod]
        public void JsonDiffWriter_BooleanValues()
        {
            var a = JsonNode.Parse("""{"a":true,"b":false}""")!.AsObject();
            var b = JsonNode.Parse("""{"a":true,"b":true}""")!.AsObject();
            ValidateDiff(a, b);

            var c = JsonNode.Parse("""{"a":false}""")!.AsObject();
            var d = JsonNode.Parse("""{"a":true}""")!.AsObject();
            ValidateDiff(c, d);
        }

        [DataTestMethod]
        [DataRow("\"normal\"", "\"changed\"")]
        [DataRow("\"with space\"", "\"with space\"")]
        [DataRow("\"with\\\"quote\"", "\"with\\\"quote\"")]
        [DataRow("\"with\\nnewline\"", "\"with\\nnewline\"")]
        [DataRow("\"with\\ttab\"", "\"with\\ttab\"")]
        [DataRow("\"with\\\\backslash\"", "\"with\\\\backslash\"")]
        [DataRow("\"unicode\\u00E9\"", "\"unicode\\u00E9\"")]
        [DataRow("\"line1\\nline2\\nline3\"", "\"line1\\nline2\\nline2\"")]
        [DataRow("\"\\\"\"", "\"changed\"")]
        public void JsonDiffWriter_StringEncoding(string sourceValue, string targetValue)
        {
            var a = JsonNode.Parse($"{{\"prop\": {sourceValue}}}")!.AsObject();
            var b = JsonNode.Parse($"{{\"prop\": {targetValue}}}")!.AsObject();
            ValidateDiff(a, b);
        }

        [DataTestMethod]
        [DataRow("simple", "\"changed\"")]
        [DataRow("with space", "\"changed\"")]
        [DataRow("with\\\"quote", "\"changed\"")]
        [DataRow("with\\nnewline", "\"changed\"")]
        [DataRow("with\\ttab", "\"changed\"")]
        [DataRow("with\\\\backslash", "\"changed\"")]
        [DataRow("unicodeé", "\"changed\"")]
        [DataRow("$type", "\"changed\"")]
        [DataRow("", "\"changed\"")]
        public void JsonDiffWriter_PropertyNameEncoding(string propertyName, string targetValue)
        {
            var sourceJson = $"{{\"{propertyName}\": \"value\"}}";
            var targetJson = $"{{\"{propertyName}\": {targetValue}}}";
            var a = JsonNode.Parse(sourceJson)!.AsObject();
            var b = JsonNode.Parse(targetJson)!.AsObject();
            ValidateDiff(a, b);
        }

        [DataTestMethod]
        [DataRow(0, 1)]
        [DataRow(-1, 1)]
        [DataRow(1.5, 2.5)]
        [DataRow(1000, 9999)]
        [DataRow(-1000, -9999)]
        [DataRow(0.0001, 0.0002)]
        [DataRow(1e100, 2e100)]
        public void JsonDiffWriter_NumberValues(double sourceValue, double targetValue)
        {
            var a = JsonNode.Parse($"{{\"prop\": {sourceValue}}}")!.AsObject();
            var b = JsonNode.Parse($"{{\"prop\": {targetValue}}}")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_NumberEquality()
        {
            var a = JsonNode.Parse("""{"a":1.0,"b":2.0,"c":-1.0}""")!.AsObject();
            var b = JsonNode.Parse("""{"a":1.0,"b":2.0,"c":-1.0}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_NestedObjects()
        {
            var a = JsonNode.Parse("""{"a":{"b":{"c":1},"d":2},"e":3}""")!.AsObject();
            var b = JsonNode.Parse("""{"a":{"b":{"c":2},"d":2},"e":3}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_NestedObjectsEmpty()
        {
            var a = JsonNode.Parse("""{"a":{}}""")!.AsObject();
            var b = JsonNode.Parse("""{"a":{"b":1}}""")!.AsObject();
            ValidateDiff(a, b);

            var c = JsonNode.Parse("""{"a":{"b":1}}""")!.AsObject();
            var d = JsonNode.Parse("""{"a":{}}""")!.AsObject();
            ValidateDiff(c, d);
        }

        [DataTestMethod]
        [DataRow(5, 0)]
        [DataRow(5, 1)]
        [DataRow(10, 0)]
        [DataRow(5, 9)]
        [DataRow(10, 4)]
        public void JsonDiffWriter_ArrayDifferentOneElement(int arraySize, int changeIndex)
        {
            var sourceArray = string.Join(",", Enumerable.Range(0, arraySize).Select(i => i.ToString()));
            var targetArray = string.Join(",", Enumerable.Range(0, arraySize).Select(i => i == changeIndex ? "-1" : i.ToString()));
            var a = JsonNode.Parse($"{{\"arr\": [{sourceArray}]}}")!.AsObject();
            var b = JsonNode.Parse($"{{\"arr\": [{targetArray}]}}")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_ArraySame()
        {
            var a = JsonNode.Parse("""{"arr":[1,2,3,4,5]}""")!.AsObject();
            var b = JsonNode.Parse("""{"arr":[1,2,3,4,5]}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [DataTestMethod]
        [DataRow(0, 1)]
        [DataRow(0, 3)]
        [DataRow(3, 0)]
        [DataRow(0, 0)]
        [DataRow(1, 2)]
        [DataRow(2, 5)]
        [DataRow(1, 10)]
        public void JsonDiffWriter_ArrayLengthChanged(int sourceLength, int targetLength)
        {
            var sourceArray = string.Join(",", Enumerable.Range(0, sourceLength).Select(i => i.ToString()));
            var targetArray = string.Join(",", Enumerable.Range(0, targetLength).Select(i => i.ToString()));
            var a = JsonNode.Parse($"{{\"arr\": [{sourceArray}]}}")!.AsObject();
            var b = JsonNode.Parse($"{{\"arr\": [{targetArray}]}}")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_ArrayEmpty()
        {
            var a = JsonNode.Parse("""{"arr":[]}""")!.AsObject();
            var b = JsonNode.Parse("""{"arr":[1,2,3]}""")!.AsObject();
            ValidateDiff(a, b);

            var c = JsonNode.Parse("""{"arr":[1,2,3]}""")!.AsObject();
            var d = JsonNode.Parse("""{"arr":[]}""")!.AsObject();
            ValidateDiff(c, d);
        }

        [TestMethod]
        public void JsonDiffWriter_NestedArrays()
        {
            var a = JsonNode.Parse("""{"arr":[[1,2],[3,4]]}""")!.AsObject();
            var b = JsonNode.Parse("""{"arr":[[1,2],[3,5]]}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_NestedArrays2()
        {
            var a = JsonNode.Parse("""{"arr":[[1,2],[3,4],[[1]]]}""")!.AsObject();
            var b = JsonNode.Parse("""{"arr":[[1,2],[3,5],[[1]]]}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_NestedArrays_Equal()
        {
            var a = JsonNode.Parse("""{"arr":[[1,2],[3,4],[[1]]]}""")!.AsObject();
            var b = JsonNode.Parse("""{"arr":[[1,2],[3,4],[[1]]]}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_ArrayOfObjects()
        {
            var a = JsonNode.Parse("""{"arr":[{"a":1},{"a":2},{"a":3}]}""")!.AsObject();
            var b = JsonNode.Parse("""{"arr":[{"a":1},{"a":2},{"a":4}]}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_ArrayOfObjectsNestedChange()
        {
            var a = JsonNode.Parse("""{"arr":[{"a":{"b":1}},{"a":{"b":2}},{"a":{"b":3}}]}""")!.AsObject();
            var b = JsonNode.Parse("""{"arr":[{"a":{"b":1}},{"a":{"b":2}},{"a":{"b":4}}]}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_VeryLongString()
        {
            var longString = new string('x', 10000);
            var a = JsonNode.Parse($"{{\"prop\": \"{longString}\"}}")!.AsObject();
            var b = JsonNode.Parse($"{{\"prop\": \"changed\"}}")!.AsObject();
            ValidateDiff(a, b);

            var longString2 = new string('y', 50000);
            var c = JsonNode.Parse($"{{\"prop\": \"{longString}\"}}")!.AsObject();
            var d = JsonNode.Parse($"{{\"prop\": \"{longString2}\"}}")!.AsObject();
            ValidateDiff(c, d);
        }

        [TestMethod]
        public void JsonDiffWriter_VeryLongArray()
        {
            var arr = string.Join(",", Enumerable.Range(0, 1000));
            var a = JsonNode.Parse($"{{\"arr\": [{arr}]}}")!.AsObject();
            var b = JsonNode.Parse($"{{\"arr\": [{arr}]}}")!.AsObject();
            ValidateDiff(a, b);

            var arr2 = string.Join(",", Enumerable.Range(0, 1000).Select(i => i == 999 ? "9999" : i.ToString()));
            var c = JsonNode.Parse($"{{\"arr\": [{arr}]}}")!.AsObject();
            var d = JsonNode.Parse($"{{\"arr\": [{arr2}]}}")!.AsObject();
            ValidateDiff(c, d);
        }

        [TestMethod]
        public void JsonDiffWriter_MultiplePropertiesChanged()
        {
            var a = JsonNode.Parse("""{"a":1,"b":2,"c":3,"d":4}""")!.AsObject();
            var b = JsonNode.Parse("""{"a":1,"b":20,"c":3,"d":40}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_PropertyRemoved()
        {
            var a = JsonNode.Parse("""{"a":1,"b":2,"c":3}""")!.AsObject();
            var b = JsonNode.Parse("""{"a":1,"c":3}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_PropertyAdded()
        {
            var a = JsonNode.Parse("""{"a":1,"c":3}""")!.AsObject();
            var b = JsonNode.Parse("""{"a":1,"b":2,"c":3}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_DeeplyNested()
        {
            var a = JsonNode.Parse("""{"a":{"b":{"c":{"d":{"e":1}}}}}""")!.AsObject();
            var b = JsonNode.Parse("""{"a":{"b":{"c":{"d":{"e":2}}}}}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_TypePropertyAlwaysIncluded()
        {
            var a = JsonNode.Parse("""{"$type":"test","value":1}""")!.AsObject();
            var b = JsonNode.Parse("""{"$type":"test","value":2}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_MixedArraysAndObjects()
        {
            var a = JsonNode.Parse("""{"data":[{"items":[1,2,3]},{"items":[4,5,6]}]}""")!.AsObject();
            var b = JsonNode.Parse("""{"data":[{"items":[1,2,4]},{"items":[4,5,6]}]}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_EscapedCharactersInPropertyNames()
        {
            var a = JsonNode.Parse("""{"a\"b":1,"a\\b":2,"a\/b":3}""")!.AsObject();
            var b = JsonNode.Parse("""{"a\"b":1,"a\\b":20,"a\/b":30}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_EscapedCharactersInStrings()
        {
            var a = JsonNode.Parse("""{"prop":"line1\nline2\ttab"}""")!.AsObject();
            var b = JsonNode.Parse("""{"prop":"line1\nline2\tchanged"}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_Utf8Characters()
        {
            var a = JsonNode.Parse("""{"prop":"hello světe مرحبا 世界"}""")!.AsObject();
            var b = JsonNode.Parse("""{"prop":"hello 世界 مرحبا"}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_LargeNestedStructure()
        {
            var builder = new StringBuilder();
            builder.Append("{\"root\":{");
            for (int i = 0; i < 50; i++)
            {
                builder.Append($"\"prop{i}\": {{");
                for (int j = 0; j < 10; j++)
                {
                    builder.Append($"\"sub{j}\": {i * 10 + j},");
                }
                builder.Append("\"end\": 1},");
            }
            builder.Append("\"last\": true}}");

            var a = JsonNode.Parse(builder.ToString())!.AsObject();
            var b = JsonNode.Parse(builder.ToString().Replace("\"end\": 1", "\"end\": 9"))!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_ArrayWithDifferentTypes()
        {
            var a = JsonNode.Parse("""{"arr":[1,"two",true,null]}""")!.AsObject();
            var b = JsonNode.Parse("""{"arr":[1,"three",true,null]}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_NegativeNumbers()
        {
            var a = JsonNode.Parse("""{"a":-1,"b":-100.5,"c":-0.001}""")!.AsObject();
            var b = JsonNode.Parse("""{"a":-2,"b":-100.5,"c":-0.002}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_ObjectInArrayInObject()
        {
            var a = JsonNode.Parse("""{"wrapper":[{"id":1,"data":{"x":1,"y":2}},{"id":2,"data":{"x":3,"y":4}}]}""")!.AsObject();
            var b = JsonNode.Parse("""{"wrapper":[{"id":1,"data":{"x":1,"y":3}},{"id":2,"data":{"x":3,"y":4}}]}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [DataTestMethod]
        [DataRow(5)]
        [DataRow(100)]
        [DataRow(1000)]
        public void JsonDiffWriter_ArrayPerformanceTestLarge(int size)
        {
            var sourceArray = string.Join(",", Enumerable.Range(0, size).Select(i => i.ToString()));
            var targetArray = string.Join(",", Enumerable.Range(0, size).Select(i => i == size / 2 ? (i + 1000).ToString() : i.ToString()));
            var a = JsonNode.Parse($"{{\"arr\": [{sourceArray}]}}")!.AsObject();
            var b = JsonNode.Parse($"{{\"arr\": [{targetArray}]}}")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_ArrayObjectToPrimitiveChange()
        {
            var a = JsonNode.Parse("""{"arr":[{"a":1}]}""")!.AsObject();
            var b = JsonNode.Parse("""{"arr":["string"]}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_ArrayPrimitiveToObjectChange()
        {
            var a = JsonNode.Parse("""{"arr":["primitive"]}""")!.AsObject();
            var b = JsonNode.Parse("""{"arr":[{"new":1}]}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_EmptyObjectInArray()
        {
            var a = JsonNode.Parse("""{"arr":[{}]}""")!.AsObject();
            var b = JsonNode.Parse("""{"arr":[{"a":1}]}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_ObjectToArrayChange()
        {
            var a = JsonNode.Parse("""{"prop":{"old":1}}""")!.AsObject();
            var b = JsonNode.Parse("""{"prop":[1,2,3]}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_NestedArraysWithPartialChanges()
        {
            var a = JsonNode.Parse("""{"arr":[[1,2,3],[4,5,6],[7,8,9]]}""")!.AsObject();
            var b = JsonNode.Parse("""{"arr":[[1,2,3],[4,99,6],[7,8,9]]}""")!.AsObject();
            ValidateDiff(a, b);
        }

        [TestMethod]
        public void JsonDiffWriter_EmptyTargetObject()
        {
            var a = JsonNode.Parse("""{"a":1,"b":2,"c":3}""")!.AsObject();
            var b = JsonNode.Parse("""{}""")!.AsObject();
            ValidateDiff(a, b);
        }
    }
}
