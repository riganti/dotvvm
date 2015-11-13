using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class RedirectTests : SeleniumTestBase
    {
        [TestMethod]
        public void RedirectTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("FeatureSamples/Redirect");

                var originalUrl = browser.CurrentUrl;
                browser.CheckUrl(s => s.Contains("?time="), "Current url doesn't contain query string ?time=");

                // click the button
                browser.First("input[type=button]").Click().Wait();
                browser.CheckUrl(s => !s.Equals(originalUrl, StringComparison.OrdinalIgnoreCase),
                    "Current url is same as origional url. Current url should be different.");
            });
        }
    }
}