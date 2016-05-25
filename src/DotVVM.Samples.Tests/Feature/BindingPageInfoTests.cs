using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;
using DotVVM.Framework.Routing;

namespace DotVVM.Samples.Tests.Feature
{   
    [TestClass]
    public class BindingPageInfoTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_BindingPageInfoTests()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_BindingPageInfo_BindingPageInfo);

                // verify the first date
                browser.ElementAt("p", 1).CheckIfInnerText(s => s.Contains("server"));
                browser.ElementAt("p", 2).CheckIfInnerText(s => s.Contains("client"));

                browser.ElementAt("input[type=button]", 0).Click();
                browser.ElementAt("p", 0).CheckIfInnerText(s => s.Contains("running"));
            });
        }
    }
}