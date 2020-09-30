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
            var configuration = DotvvmTestHelper.CreateConfiguration();
            var manager = new ResourceManager(configuration.Resources);

            manager.AddRequiredResource(ResourceConstants.GlobalizeResourceName);
            Assert.AreEqual(configuration.Resources.FindResource(ResourceConstants.GlobalizeResourceName), manager.GetResourcesInOrder().First());
        }

        [TestMethod]
        public void ResourceManager_DependentResources()
        {
            var configuration = DotvvmTestHelper.CreateConfiguration();
            var manager = new ResourceManager(configuration.Resources);

            manager.AddRequiredResource(ResourceConstants.DotvvmResourceName);
            var resourcesInCorrectOrder = manager.GetResourcesInOrder().ToList();
            Assert.AreEqual(configuration.Resources.FindResource(ResourceConstants.KnockoutJSResourceName), resourcesInCorrectOrder[0]);
            Assert.AreEqual(configuration.Resources.FindResource(ResourceConstants.PolyfillResourceName), resourcesInCorrectOrder[1]);
            Assert.AreEqual(configuration.Resources.FindResource(ResourceConstants.DotvvmResourceName + ".internal"), resourcesInCorrectOrder[2]);
            Assert.AreEqual(configuration.Resources.FindResource(ResourceConstants.DotvvmResourceName), resourcesInCorrectOrder[3]);
        }

        [TestMethod]
        public void ResourceManager_DependentResources_Css()
        {
            var configuration = DotvvmTestHelper.CreateConfiguration();
            var manager = new ResourceManager(configuration.Resources);

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
            //define
            var config1 = DotvvmTestHelper.CreateConfiguration();
            config1.Resources.Register("rs1", new ScriptResource(new FileResourceLocation("file.js")));
            config1.Resources.Register("rs2", new StylesheetResource(new UrlResourceLocation("http://c.c/")));
            config1.Resources.Register("rs3", new StylesheetResource(new EmbeddedResourceLocation(typeof(DotvvmConfiguration).GetTypeInfo().Assembly, "DotVVM.Framework.Resources.Scripts.knockout-latest.js", "../file.js")));
            config1.Resources.Register("rs4", new InlineScriptResource("CODE", ResourceRenderPosition.Head));
            config1.Resources.Register("rs5", new NullResource());
            config1.Resources.Register("rs6", new ScriptResource(
                new UrlResourceLocation("http://d.d/"))
            {
                LocationFallback = new ResourceLocationFallback("condition", new FileResourceLocation("file1.js"))
            });
            config1.Resources.Register("rs7", new PolyfillResource(){ RenderPosition =  ResourceRenderPosition.Head});
            config1.Resources.Register("rs8", new ScriptResource(new JQueryGlobalizeResourceLocation(CultureInfo.GetCultureInfo("en-US"))));

            var settings = DefaultSerializerSettingsProvider.Instance.GetSettingsCopy();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            var config2 = JsonConvert.DeserializeObject<DotvvmConfiguration>(JsonConvert.SerializeObject(config1, settings), settings);

            //test 
            Assert.IsTrue(config2.Resources.FindResource("rs1") is ScriptResource rs1 &&
                rs1.Location is FileResourceLocation rs1loc &&
                rs1loc.FilePath == "file.js");
            Assert.IsTrue(config2.Resources.FindResource("rs2") is StylesheetResource rs2 &&
                rs2.Location is UrlResourceLocation rs2loc &&
                rs2loc.Url == "http://c.c/");
            Assert.IsTrue(config2.Resources.FindResource("rs3") is StylesheetResource rs3 &&
                rs3.Location is EmbeddedResourceLocation rs3loc &&
                rs3loc.Assembly.GetName().Name == "DotVVM.Framework" &&
                rs3loc.Name == "DotVVM.Framework.Resources.Scripts.knockout-latest.js"&&
                rs3loc.DebugFilePath == "../file.js");
            Assert.IsTrue(config2.Resources.FindResource("rs4") is InlineScriptResource rs4 &&
                rs4.RenderPosition == ResourceRenderPosition.Head &&
                rs4.Code == "CODE");
            Assert.IsTrue(config2.Resources.FindResource("rs5") is NullResource);
            Assert.IsTrue(config2.Resources.FindResource("rs6") is ScriptResource rs6 &&
                rs6.Location is UrlResourceLocation rs6loc && rs6loc.Url == "http://d.d/" &&
                rs6.LocationFallback.JavascriptCondition == "condition" &&
                rs6.LocationFallback.AlternativeLocations.Single() is FileResourceLocation rs6loc2 &&
                rs6loc2.FilePath == "file1.js");
            Assert.IsTrue(config2.Resources.FindResource("rs7") is PolyfillResource rs7 && rs7.RenderPosition == ResourceRenderPosition.Head);
            Assert.IsTrue(config2.Resources.FindResource("rs8") is ScriptResource rs8 && rs8.Location is JQueryGlobalizeResourceLocation rs8loc);
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
}}", ResourceConstants.GlobalizeResourceName);
            var configuration = DotvvmTestHelper.CreateConfiguration();
            JsonConvert.PopulateObject(json.Replace("'", "\""), configuration);

            Assert.IsTrue(configuration.Resources.FindResource(ResourceConstants.GlobalizeResourceName) is ScriptResource);
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
