using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using System.Text.Json;
using DotVVM.Framework.Hosting;

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
            var diff = JsonUtils.Diff(a, b);
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
            var diff = JsonUtils.Diff(source, target);
            Assert.AreEqual("""{"FieldName":"Code","Operator":"Equal","Value":"HHK"}""", diff.ToJsonString());
        }

        private JsonObject CreateDiff(Action<DotvvmConfiguration> fn)
        {
            var config = DotvvmTestHelper.CreateConfiguration();
            var json0 = JsonSerializer.SerializeToNode(config, serializerOptions)!.AsObject();
            fn(config);
            var json1 = JsonSerializer.SerializeToNode(config, serializerOptions)!.AsObject();
            return JsonUtils.Diff(json0, json1);
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
    }
}
