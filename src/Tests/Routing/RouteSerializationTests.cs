using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace DotVVM.Framework.Tests.Routing
{
    [TestClass]
    public class RouteSerializationTests
    {
        [TestMethod]
        public void RouteTable_Deserialization()
        {
            DotvvmTestHelper.EnsureCompiledAssemblyCache();

            var config1 = DotvvmTestHelper.CreateConfiguration();
            config1.RouteTable.Add("route1", "url1", "file1.dothtml", new { a = "ccc" });
            config1.RouteTable.Add("route2", "url2/{int:posint}", "file1.dothtml", new { a = "ccc" });

            // Add unknown constraint, simulate user defined constraint that is not known to the VS Extension
            var r = new DotvvmRoute("url3", "file1.dothtml", new { }, provider => null, config1);
            typeof(RouteBase).GetProperty("Url").SetMethod.Invoke(r, new[] { "url3/{a:unsuppotedConstraint}" });
            config1.RouteTable.Add("route3", r);

            var settings = DefaultSerializerSettingsProvider.Instance.GetSettingsCopy();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            var config2 = JsonConvert.DeserializeObject<DotvvmConfiguration>(JsonConvert.SerializeObject(config1, settings), settings);

            Assert.AreEqual(config2.RouteTable["route1"].Url, "url1");
            Assert.AreEqual(config2.RouteTable["route2"].Url, "url2/{int:posint}");
            Assert.AreEqual(config2.RouteTable["route3"].Url, "url3/{a:unsuppotedConstraint}");
        }
    }
}
