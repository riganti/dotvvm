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
    public class NestedRepeaterTests : SeleniumTest
    {
        [TestMethod]
        public void Control_NestedRepeater()
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
    }
}