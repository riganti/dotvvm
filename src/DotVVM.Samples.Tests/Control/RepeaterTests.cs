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

        [TestMethod]
        public void Control_Repeater_NestedRepeater()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_NestedRepeater);
                browser.Wait();

                browser.ElementAt("a", 0).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 1 Subchild 1");

                browser.ElementAt("a", 1).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 1 Subchild 2");

                browser.ElementAt("a", 2).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 1 Subchild 3");

                browser.ElementAt("a", 3).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 2 Subchild 1");

                browser.ElementAt("a", 4).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 2 Subchild 2");

                browser.ElementAt("a", 5).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 3 Subchild 1");

                browser.ElementAt("a", 6).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 1 Subchild 1");

                browser.ElementAt("a", 7).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 1 Subchild 2");

                browser.ElementAt("a", 8).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 1 Subchild 3");

                browser.ElementAt("a", 9).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 2 Subchild 1");

                browser.ElementAt("a", 10).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 2 Subchild 2");

                browser.ElementAt("a", 11).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 3 Subchild 1");
            });
        }

        [TestMethod]
        public void Control_Repeater_RepeaterWrapperTag()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RepeaterWrapperTag);

                browser.FindElements("#part1>div").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part1>div>p").ThrowIfDifferentCountThan(4);

                browser.ElementAt("#part1>div>p", 0).CheckIfInnerTextEquals("Test 1");
                browser.ElementAt("#part1>div>p", 1).CheckIfInnerTextEquals("Test 2");
                browser.ElementAt("#part1>div>p", 2).CheckIfInnerTextEquals("Test 3");
                browser.ElementAt("#part1>div>p", 3).CheckIfInnerTextEquals("Test 4");

                browser.FindElements("#part2>ul").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part2>ul>li").ThrowIfDifferentCountThan(4);

                browser.ElementAt("#part2>ul>li", 0).CheckIfInnerTextEquals("Test 1");
                browser.ElementAt("#part2>ul>li", 1).CheckIfInnerTextEquals("Test 2");
                browser.ElementAt("#part2>ul>li", 2).CheckIfInnerTextEquals("Test 3");
                browser.ElementAt("#part2>ul>li", 3).CheckIfInnerTextEquals("Test 4");

                browser.FindElements("#part3>p").ThrowIfDifferentCountThan(4);

                browser.ElementAt("#part3>p", 0).CheckIfInnerTextEquals("Test 1");
                browser.ElementAt("#part3>p", 1).CheckIfInnerTextEquals("Test 2");
                browser.ElementAt("#part3>p", 2).CheckIfInnerTextEquals("Test 3");
                browser.ElementAt("#part3>p", 3).CheckIfInnerTextEquals("Test 4");

                browser.FindElements("#part1_server>div").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part1_server>div>p").ThrowIfDifferentCountThan(4);

                browser.ElementAt("#part1_server>div>p", 0).CheckIfInnerTextEquals("Test 1");
                browser.ElementAt("#part1_server>div>p", 1).CheckIfInnerTextEquals("Test 2");
                browser.ElementAt("#part1_server>div>p", 2).CheckIfInnerTextEquals("Test 3");
                browser.ElementAt("#part1_server>div>p", 3).CheckIfInnerTextEquals("Test 4");

                browser.FindElements("#part2_server>ul").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part2_server>ul>li").ThrowIfDifferentCountThan(4);

                browser.ElementAt("#part2_server>ul>li", 0).CheckIfInnerTextEquals("Test 1");
                browser.ElementAt("#part2_server>ul>li", 1).CheckIfInnerTextEquals("Test 2");
                browser.ElementAt("#part2_server>ul>li", 2).CheckIfInnerTextEquals("Test 3");
                browser.ElementAt("#part2_server>ul>li", 3).CheckIfInnerTextEquals("Test 4");

                browser.FindElements("#part3_server>p").ThrowIfDifferentCountThan(4);
                browser.ElementAt("#part3_server>p", 0).CheckIfInnerTextEquals("Test 1");
                browser.ElementAt("#part3_server>p", 1).CheckIfInnerTextEquals("Test 2");
                browser.ElementAt("#part3_server>p", 2).CheckIfInnerTextEquals("Test 3");
                browser.ElementAt("#part3_server>p", 3).CheckIfInnerTextEquals("Test 4");
            });
        }
    }
}