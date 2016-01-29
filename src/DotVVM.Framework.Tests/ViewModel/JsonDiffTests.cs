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
                CreateDiff(c => c.Resources.Register("resource-1", new InlineScriptResource { Code = "alert()" })),
                CreateDiff(c => c.Resources.Register("resource-2", new InlineScriptResource { Code = "console.log()" })),
                CreateDiff(c => c.Resources.Register("resource-3", new ScriptResource { Url = "http://i.dont.know/which.js", Dependencies = new[] { "dotvvm" } }))
                );
            Assert.IsInstanceOfType(config.Resources.FindResource("resource-1"), typeof(InlineScriptResource));
            Assert.IsInstanceOfType(config.Resources.FindResource("resource-2"), typeof(InlineScriptResource));
            Assert.IsInstanceOfType(config.Resources.FindResource("resource-3"), typeof(ScriptResource));
            Assert.AreEqual("dotvvm", (config.Resources.FindResource("resource-3") as ScriptResource).Dependencies.Single());
        }

        private JObject CreateDiff(Action<DotvvmConfiguration> fn)
        {
            var config = DotvvmConfiguration.CreateDefault();
            var json0 = JObject.FromObject(config);
            fn(config);
            var json1 = JObject.FromObject(config);
            return JsonUtils.Diff(json0, json1);
        }

        private DotvvmConfiguration ApplyPatches(DotvvmConfiguration init, params JObject[] patches)
        {
            var json = JObject.FromObject(init);
            foreach (var p in patches)
            {
                JsonUtils.Patch(json, p);
            }
            return json.ToObject<DotvvmConfiguration>();
        }

        private DotvvmConfiguration ApplyPatches(params JObject[] patches) => ApplyPatches(DotvvmConfiguration.CreateDefault(), patches);
    }
}
