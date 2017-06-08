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
    public class BindingContextsTests : SeleniumTest
    {
        [TestMethod]
        public void Feature_BindingContextsTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_BindingContexts_BindingContext);
                browser.Wait(1000);

                var linkCount = browser.FindElements("a").Count;
                for (var i = 0; i < linkCount; i++)
                {
                    var link = browser.ElementAt("a", i);
                    link.Click().Wait(500);
                    
                    browser.Single(".result").CheckIfInnerTextEquals(link.GetInnerText());
                }
            });
        }
    }
}