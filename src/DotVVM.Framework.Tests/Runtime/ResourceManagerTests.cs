using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Parser;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.ResourceManagement.ClientGlobalize;
using System.Globalization;

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

            manager.AddRequiredResource(Constants.JQueryResourceName);
            Assert.AreEqual(configuration.Resources.FindResource(Constants.JQueryResourceName), manager.GetResourcesInOrder().First());
        }


        [TestMethod]
        public void ResourceManager_DependentResources()
        {
            var configuration = DotvvmConfiguration.CreateDefault();
            var manager = new ResourceManager(configuration);

            manager.AddRequiredResource(Constants.DotvvmResourceName);
            var resourcesInCorrectOrder = manager.GetResourcesInOrder().ToList();
            Assert.AreEqual(configuration.Resources.FindResource(Constants.KnockoutJSResourceName), resourcesInCorrectOrder[0]);
            Assert.AreEqual(configuration.Resources.FindResource(Constants.DotvvmResourceName), resourcesInCorrectOrder[1]);
        }


        [TestMethod]
        public void ResourceManager_DependentResources_Css()
        {
            var configuration = DotvvmConfiguration.CreateDefault();
            var manager = new ResourceManager(configuration);

            manager.AddRequiredResource(Constants.DotvvmFileUploadCssResourceName);
            var resourcesInCorrectOrder = manager.GetResourcesInOrder().ToList();
            Assert.AreEqual(configuration.Resources.FindResource(Constants.KnockoutJSResourceName), resourcesInCorrectOrder[0]);
            Assert.AreEqual(configuration.Resources.FindResource(Constants.DotvvmResourceName), resourcesInCorrectOrder[1]);
            Assert.AreEqual(configuration.Resources.FindResource(Constants.DotvvmFileUploadResourceName), resourcesInCorrectOrder[2]);
            Assert.AreEqual(configuration.Resources.FindResource(Constants.DotvvmFileUploadCssResourceName), resourcesInCorrectOrder[3]);
        }

        /// <summary>
        /// Verifies that the default configuration populated with contents from the JSON file is merged correctly.
        /// </summary>
        [TestMethod]
        public void ResourceManager_ConfigurationDeserialization()
        {
            var json = string.Format(@"
{{ 
    'resources': {{
        'scripts': {{ '{0}': {{ 'url': 'different url', 'globalObjectName': '$'}} }},
        'stylesheets': {{ 'newResource': {{ 'url': 'test' }} }}
    }}
}}", Constants.JQueryResourceName);
            var configuration = DotvvmConfiguration.CreateDefault();
            JsonConvert.PopulateObject(json.Replace("'", "\""), configuration);

            Assert.AreEqual("different url", configuration.Resources.FindResource(Constants.JQueryResourceName).Url);
            Assert.AreEqual("test", configuration.Resources.FindResource("newResource").Url);
        }

        [TestMethod]
        public void JQueryGlobalizeGenerator()
        {
            var cultureInfo = CultureInfo.GetCultureInfo("cs-cz");
            var json = JQueryGlobalizeScriptCreator.BuildCultureInfoJson(cultureInfo);
            Assert.IsTrue(json.SelectToken("calendars.standard.days.namesAbbr").Values<string>().SequenceEqual(cultureInfo.DateTimeFormat.AbbreviatedDayNames));
            // TODO: add more assertions
        }
    }
}
