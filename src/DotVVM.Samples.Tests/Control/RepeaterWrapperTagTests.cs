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
    public class RepeaterWrapperTagTests : SeleniumTest
    {
        [TestMethod]
        public void Control_RepeaterWrapperTag()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RepeaterWrapperTag_RepeaterWrapperTag);
                
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