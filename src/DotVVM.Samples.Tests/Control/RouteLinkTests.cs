using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class RouteLinkTests : SeleniumTestBase
    {
        [TestMethod]
        public void Control_RouteLink()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RouteLink);
                
                // verify link urls
                browser.CheckUrl(s => browser.ElementAt("a", 0).GetAttribute("href").Equals(s + "/1"),
                    "Wrong set route");
                browser.CheckUrl(s => browser.ElementAt("a", 1).GetAttribute("href").Equals(s + "/2"),
                    "Wrong set route");
                browser.CheckUrl(s => browser.ElementAt("a", 2).GetAttribute("href").Equals(s + "/3"),
                    "Wrong set route");
                browser.CheckUrl(s => browser.ElementAt("a", 3).GetAttribute("href").Equals(s + "/1"),
                    "Wrong set route");
                browser.CheckUrl(s => browser.ElementAt("a", 4).GetAttribute("href").Equals(s + "/2"),
                    "Wrong set route");
                browser.CheckUrl(s => browser.ElementAt("a", 5).GetAttribute("href").Equals(s + "/3"),
                    "Wrong set route");
                browser.CheckUrl(s => browser.ElementAt("a", 6).GetAttribute("href").Equals(s + "/1"),
                    "Wrong set route");
                browser.CheckUrl(s => browser.ElementAt("a", 7).GetAttribute("href").Equals(s + "/2"),
                    "Wrong set route");
                browser.CheckUrl(s => browser.ElementAt("a", 8).GetAttribute("href").Equals(s + "/3"),
                    "Wrong set route");
                browser.CheckUrl(s => browser.ElementAt("a", 9).GetAttribute("href").Equals(s + "/1"),
                    "Wrong set route");
                browser.CheckUrl(s => browser.ElementAt("a", 10).GetAttribute("href").Equals(s + "/2"),
                    "Wrong set route");
                browser.CheckUrl(s => browser.ElementAt("a", 11).GetAttribute("href").Equals(s + "/3"),
                    "Wrong set route");

                for (int i = 0; i < 12; i++)
                {
                    browser.ElementAt("a", i).CheckIfInnerText(s => !string.IsNullOrWhiteSpace(s),
                        "Not rendered Name");
                }
            });
        }
    }
}