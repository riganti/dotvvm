using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class JsonDiffTests
    {

        private JsonSerializer serializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.Auto };

        public JsonDiffTests()
        {
            DotvvmTestHelper.EnsureCompiledAssemblyCache();
        }

        [TestMethod]
        public void JsonDiff_SimpleTest()
        {
            var a = JObject.Parse("{name:'djsfsh',ahoj:45}");
            var b = JObject.Parse("{name:'djsfsh',ahoj:42}");
            var diff = JsonUtils.Diff(a, b);
            JsonUtils.Patch(a, diff);
            Assert.IsTrue(JToken.DeepEquals(a, b));
        }

        [TestMethod]
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
        public void JsonDiff_Configuration_AddingRoute()
        {
            var config = ApplyPatches(
                CreateDiff(c => c.RouteTable.Add("Route1", "Path1", "View1.dothtml")),
                CreateDiff(c => c.RouteTable.Add("Route2", "Path2", "View2.dothtml")),
                CreateDiff(c => c.RouteTable.Add("Route3", "Path3/{Name}", "View3.dothtml", new { Name = "defaultname" }))
                );
            Assert.IsTrue(config.RouteTable.Any(r => r.RouteName == "Route1"));
            Assert.IsTrue(config.RouteTable.Any(r => r.RouteName == "Route2"));
            Assert.IsTrue(config.RouteTable.Any(r => r.RouteName == "Route3"));
            Assert.AreEqual("View1.dothtml", config.RouteTable.Single(r => r.RouteName == "Route1").VirtualPath);
            Assert.AreEqual("defaultname", config.RouteTable.Single(r => r.RouteName == "Route3").DefaultValues["Name"]);
        }

        [TestMethod]
        public void JsonDiff_BusinessPackFilter_NoThrow()
        {
            var source = JObject.Parse(
@"{
	""FieldName"": ""DateRequiredBy"",
	""FieldDisplayName"": null,
	""Operator"": ""LessThan"",
	""FormatString"": null,
	""Value"": ""2019-08-07T00:00:00"",
	""Type"": ""FilterCondition""
}");
            var target = JObject.Parse(
@"{
	""FieldName"": ""Code"",
	""FieldDisplayName"": null,
	""Operator"": ""Equal"",
	""FormatString"": null,
	""Value"": ""HHK"",
	""Type"": ""FilterCondition""
}");
            var diff = JsonUtils.Diff(source, target);
        }

        private JObject CreateDiff(Action<DotvvmConfiguration> fn)
        {
            var config = DotvvmTestHelper.CreateConfiguration();
            var json0 = JObject.FromObject(config, serializer);
            fn(config);
            var json1 = JObject.FromObject(config, serializer);
            return JsonUtils.Diff(json0, json1);
        }

        private DotvvmConfiguration ApplyPatches(DotvvmConfiguration init, params JObject[] patches)
        {
            var json = JObject.FromObject(init, serializer);
            foreach (var p in patches)
            {
                JsonUtils.Patch(json, p);
            }
            return json.ToObject<DotvvmConfiguration>(serializer);
        }

        private DotvvmConfiguration ApplyPatches(params JObject[] patches) => ApplyPatches(DotvvmTestHelper.CreateConfiguration(), patches);
    }
}
