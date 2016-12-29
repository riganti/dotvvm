using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.ResourceManagement.ClientGlobalize;
using System.Globalization;
using DotVVM.Framework.Compilation.Parser;
using System.Reflection;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class ResourceManagerTests
    {

        [TestMethod]
        public void ResourceManager_SimpleTest()
        {
            var configuration = DotvvmConfiguration.CreateDefault();
            var manager = new ResourceManager(configuration);

            manager.AddRequiredResource(ResourceConstants.JQueryResourceName);
            Assert.AreEqual(configuration.Resources.FindResource(ResourceConstants.JQueryResourceName), manager.GetResourcesInOrder().First());
        }


        [TestMethod]
        public void ResourceManager_DependentResources()
        {
            var configuration = DotvvmConfiguration.CreateDefault();
            var manager = new ResourceManager(configuration);

            manager.AddRequiredResource(ResourceConstants.DotvvmResourceName);
            var resourcesInCorrectOrder = manager.GetResourcesInOrder().ToList();
            Assert.AreEqual(configuration.Resources.FindResource(ResourceConstants.KnockoutJSResourceName), resourcesInCorrectOrder[0]);
            Assert.AreEqual(configuration.Resources.FindResource(ResourceConstants.DotvvmResourceName + ".internal"), resourcesInCorrectOrder[1]);
            Assert.AreEqual(configuration.Resources.FindResource(ResourceConstants.DotvvmResourceName), resourcesInCorrectOrder[2]);
        }


        [TestMethod]
        public void ResourceManager_DependentResources_Css()
        {
            var configuration = DotvvmConfiguration.CreateDefault();
            var manager = new ResourceManager(configuration);

            manager.AddRequiredResource(ResourceConstants.DotvvmFileUploadCssResourceName);
            var resourcesInCorrectOrder = manager.GetResourcesInOrder().ToList();
            Assert.AreEqual(configuration.Resources.FindResource(ResourceConstants.DotvvmFileUploadCssResourceName), resourcesInCorrectOrder[0]);
        }

        /// <summary>
        /// Verifies that the default configuration populated with contents from the JSON file is merged correctly.
        /// </summary>
        [TestMethod]
        public void ResourceManager_ConfigurationDeserialization()
        {
            var config1 = DotvvmConfiguration.CreateDefault();
            config1.Resources.Register("rs1", new ScriptResource(new LocalFileResourceLocation("file.js")));
            config1.Resources.Register("rs2", new StylesheetResource(new RemoteResourceLocation("http://c.c/")));
            config1.Resources.Register("rs3", new StylesheetResource(new EmbeddedResourceLocation(typeof(DotvvmConfiguration).GetTypeInfo().Assembly, "DotVVM.Framework.Resources.Scripts.jquery-2.1.1.min.js", "../file.js")));
            config1.Resources.Register("rs4", new InlineScriptResource(ResourceRenderPosition.Head) { Code = "CODE" });
            config1.Resources.Register("rs5", new NullResource());
            config1.Resources.Register("rs6", new ScriptResource(
                new RemoteResourceLocation("http://d.d/"))
            {
                LocationFallback = new ResourceLocationFallback("condition", new LocalFileResourceLocation("file1.js"))
            });

            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var config2 = JsonConvert.DeserializeObject<DotvvmConfiguration>(JsonConvert.SerializeObject(config1, settings), settings);

            Assert.IsTrue(config2.Resources.FindResource("rs1") is ScriptResource rs1 &&
                rs1.Location is LocalFileResourceLocation rs1loc &&
                rs1loc.FilePath == "file.js");
            Assert.IsTrue(config2.Resources.FindResource("rs2") is StylesheetResource rs2 &&
                rs2.Location is RemoteResourceLocation rs2loc &&
                rs2loc.Url == "http://c.c/");
            Assert.IsTrue(config2.Resources.FindResource("rs3") is StylesheetResource rs3 &&
                rs3.Location is EmbeddedResourceLocation rs3loc &&
                rs3loc.Assembly.GetName().Name == "DotVVM.Framework" &&
                rs3loc.Name == "DotVVM.Framework.Resources.Scripts.jquery-2.1.1.min.js"&&
                rs3loc.DebugFilePath == "../file.js");
            Assert.IsTrue(config2.Resources.FindResource("rs4") is InlineScriptResource rs4 &&
                rs4.RenderPosition == ResourceRenderPosition.Head &&
                rs4.Code == "CODE");
            Assert.IsTrue(config2.Resources.FindResource("rs5") is NullResource);
            Assert.IsTrue(config2.Resources.FindResource("rs6") is ScriptResource rs6 &&
                rs6.Location is RemoteResourceLocation rs6loc && rs6loc.Url == "http://d.d/" &&
                rs6.LocationFallback.JavascriptCondition == "condition" &&
                rs6.LocationFallback.AlternativeLocations.Single() is LocalFileResourceLocation rs6loc2 &&
                rs6loc2.FilePath == "file1.js");
        }

        [TestMethod]
        public void ResourceManager_ConfigurationOldDeserialization()
        {
            var json = string.Format(@"
{{ 
    'resources': {{
        'scripts': {{ '{0}': {{ 'url': 'different url', 'globalObjectName': '$'}} }},
        'stylesheets': {{ 'newResource': {{ 'url': 'test' }} }}
    }}
}}", ResourceConstants.JQueryResourceName);
            var configuration = DotvvmConfiguration.CreateDefault();
            JsonConvert.PopulateObject(json.Replace("'", "\""), configuration);

            Assert.IsTrue(configuration.Resources.FindResource(ResourceConstants.JQueryResourceName) is ScriptResource);
            Assert.IsTrue(configuration.Resources.FindResource("newResource") is StylesheetResource);
        }

        [TestMethod]
        public void JQueryGlobalizeGenerator()
        {
            var cultureInfo = new CultureInfo("cs-cz");
            var json = JQueryGlobalizeScriptCreator.BuildCultureInfoJson(cultureInfo);
            Assert.IsTrue(json.SelectToken("calendars.standard.days.namesAbbr").Values<string>().SequenceEqual(cultureInfo.DateTimeFormat.AbbreviatedDayNames));
            // TODO: add more assertions
        }
    }
}
