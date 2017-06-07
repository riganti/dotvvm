using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class RepeaterTests : SeleniumTest
    {
        [TestMethod]
        public void Control_Repeater_RouteLink()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RouteLink);

                // verify link urls
                var url = browser.CurrentUrl;
                browser.ElementAt("a", 0).CheckAttribute("href", url + "/1");
                browser.ElementAt("a", 1).CheckAttribute("href", url + "/2");
                browser.ElementAt("a", 2).CheckAttribute("href", url + "/3");
                browser.ElementAt("a", 3).CheckAttribute("href", url + "/1");
                browser.ElementAt("a", 4).CheckAttribute("href", url + "/2");
                browser.ElementAt("a", 5).CheckAttribute("href", url + "/3");
                browser.ElementAt("a", 6).CheckAttribute("href", url + "/1");
                browser.ElementAt("a", 7).CheckAttribute("href", url + "/2");
                browser.ElementAt("a", 8).CheckAttribute("href", url + "/3");
                browser.ElementAt("a", 9).CheckAttribute("href", url + "/1");
                browser.ElementAt("a", 10).CheckAttribute("href", url + "/2");
                browser.ElementAt("a", 11).CheckAttribute("href", url + "/3");

                for (int i = 0; i < 12; i++)
                {
                    browser.ElementAt("a", i).CheckIfInnerText(s => !string.IsNullOrWhiteSpace(s), "Not rendered Name");
                }
            });
        }

        [TestMethod]
        public void Control_Repeater_RouteLinkUrlSuffix()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RouteLinkUrlSuffix);

                // verify link urls
                var url = browser.CurrentUrl;
                browser.ElementAt("a", 0).CheckAttribute("href", url + "/1?test");
                browser.ElementAt("a", 1).CheckAttribute("href", url + "/2?test");
                browser.ElementAt("a", 2).CheckAttribute("href", url + "/3?test");
                browser.ElementAt("a", 3).CheckAttribute("href", url + "/1?test");
                browser.ElementAt("a", 4).CheckAttribute("href", url + "/2?test");
                browser.ElementAt("a", 5).CheckAttribute("href", url + "/3?test");
                browser.ElementAt("a", 6).CheckAttribute("href", url + "/1?id=1");
                browser.ElementAt("a", 7).CheckAttribute("href", url + "/2?id=2");
                browser.ElementAt("a", 8).CheckAttribute("href", url + "/3?id=3");
                browser.ElementAt("a", 9).CheckAttribute("href", url + "/1?id=1");
                browser.ElementAt("a", 10).CheckAttribute("href", url + "/2?id=2");
                browser.ElementAt("a", 11).CheckAttribute("href", url + "/3?id=3");

                for (int i = 0; i < 12; i++)
                {
                    browser.ElementAt("a", i).CheckIfInnerText(s => !string.IsNullOrWhiteSpace(s), "Not rendered Name");
                }
            });
        }
    }
}