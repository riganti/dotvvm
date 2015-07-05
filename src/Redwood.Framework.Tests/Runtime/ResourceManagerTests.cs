using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Redwood.Framework.Configuration;
using Redwood.Framework.Hosting;
using Redwood.Framework.Parser;
using Redwood.Framework.ResourceManagement;
using Redwood.Framework.ResourceManagement.ClientGlobalize;
using System.Globalization;

namespace Redwood.Framework.Tests.Runtime
{
    [TestClass]
    public class ResourceManagerTests
    { 

        [TestMethod]
        public void ResourceManager_SimpleTest()
        {
            var configuration = RedwoodConfiguration.CreateDefault();
            var manager = new ResourceManager(configuration);

            manager.AddRequiredResource(Constants.JQueryResourceName);
            Assert.AreEqual(configuration.Resources.FindResource(Constants.JQueryResourceName), manager.GetResourcesInOrder().First());
        }


        [TestMethod]
        public void ResourceManager_DependentResources()
        {
            var configuration = RedwoodConfiguration.CreateDefault();
            var manager = new ResourceManager(configuration);

            manager.AddRequiredResource(Constants.RedwoodResourceName);
            var resourcesInCorrectOrder = manager.GetResourcesInOrder().ToList();
            Assert.AreEqual(configuration.Resources.FindResource(Constants.KnockoutJSResourceName), resourcesInCorrectOrder[0]);
            Assert.AreEqual(configuration.Resources.FindResource(Constants.KnockoutMapperResourceName), resourcesInCorrectOrder[1]);
            Assert.AreEqual(configuration.Resources.FindResource(Constants.RedwoodResourceName), resourcesInCorrectOrder[2]);
        }


        [TestMethod]
        public void ResourceManager_DependentResources_Css()
        {
            var configuration = RedwoodConfiguration.CreateDefault();
            var manager = new ResourceManager(configuration);

            manager.AddRequiredResource(Constants.BootstrapResourceName);
            var resourcesInCorrectOrder = manager.GetResourcesInOrder().ToList();
            Assert.AreEqual(configuration.Resources.FindResource(Constants.BootstrapCssResourceName), resourcesInCorrectOrder[0]);
            Assert.AreEqual(configuration.Resources.FindResource(Constants.JQueryResourceName), resourcesInCorrectOrder[1]);
            Assert.AreEqual(configuration.Resources.FindResource(Constants.BootstrapResourceName), resourcesInCorrectOrder[2]);
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
            var configuration = RedwoodConfiguration.CreateDefault();
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
