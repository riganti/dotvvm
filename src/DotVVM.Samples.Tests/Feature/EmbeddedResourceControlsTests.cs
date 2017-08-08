using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;



namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class EmbeddedResourceControlsTests : SeleniumTest
    {
        [TestMethod]
        public void Feature_EmbeddedResourceControls_EmbeddedResourceControls()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_EmbeddedResourceControls_EmbeddedResourceControls);

                browser.First("input[type=button]")
                    .Check().Attribute("value", s => s.Equals("Nothing"));

                browser.First("input[type=button]").Click();

                browser.First("input[type=button]")
                    .Check().Attribute("value", s => s.Equals("This is text"));

            });
        }
        
        [TestMethod]
        public void Feature_EmbeddedResourceControls_EmbeddedResourceView()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_EmbeddedResourceControls_EmbeddedResourceView);

                browser.First("input[type=button]")
                    .Check().Attribute("value", s => s.Equals("Nothing"));

                browser.First("input[type=button]").Click();

                browser.First("input[type=button]")
                    .Check().Attribute("value", s => s.Equals("This is text"));

            });
        }
    }
}

