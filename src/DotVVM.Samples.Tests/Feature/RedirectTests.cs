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
    public class RedirectTests : SeleniumTest
    {
        [TestMethod]
        public void Feature_Redirect_Redirect()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Redirect_Redirect);

                var originalUrl = browser.CurrentUrl;
                browser.CheckUrl(s => s.Contains("?time="), "Current url doesn't contain query string ?time=");

                // click the button
                browser.First("input[type=button]").Click().Wait();
                browser.CheckUrl(s => !s.Equals(originalUrl, StringComparison.OrdinalIgnoreCase),
                    "Current url is same as origional url. Current url should be different.");
            });
        }

        [TestMethod]
        public void Feature_Redirect_RedirectToRoute_ObjectQueryStringParameters()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Redirect_Redirect);

                var originalUrl = browser.CurrentUrl;

                // click the button
                browser.ElementAt("input[type=button]", 1).Click().Wait();
                browser.CheckUrl(s => !s.Equals(originalUrl, StringComparison.OrdinalIgnoreCase),
                    "Current url is same as origional url. Current url should be different.");

                browser.CheckUrl(s => s.Contains("&time="));
                browser.CheckUrl(s => s.Contains("?param=temp"));
                browser.CheckUrl(s => s.EndsWith("#test"));
            });
        }

        [TestMethod]
        public void Feature_Redirect_RedirectToRoute_DictionaryQueryStringParameters()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Redirect_Redirect);

                var originalUrl = browser.CurrentUrl;

                // click the button
                browser.ElementAt("input[type=button]", 2).Click().Wait();

                browser.CheckUrl(s => s.EndsWith("#test"));
                browser.CheckUrl(s => !s.Equals(originalUrl + "#test", StringComparison.OrdinalIgnoreCase),
                    "Current url is same as origional url. Current url should be different.");

                browser.CheckUrl(s => s.Contains("?time="));
                
            });
        }


        [TestMethod]
        public void Feature_RedirectFromPresenter()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("FeatureSamples/Redirect/RedirectFromPresenter");
                browser.Wait();

                browser.CheckUrl(s => s.Contains("?time="), "Current url doesn't contain query string ?time=");
            });
        }
    }
}