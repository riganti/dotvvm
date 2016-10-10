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
    public class IntBoundTextBoxTests : SeleniumTestBase
    {
        [TestMethod]
        public void Control_IntBoundTextBox()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_TextBox_IntBoundTextBox);

                browser.ElementAt("input", 0).SendKeys("hello");
                browser.ElementAt("input[type=button]", 0).Click();
                browser.Wait();

                browser.ElementAt("input", 0).CheckIfInnerTextEquals("");
                browser.ElementAt("span", 0).CheckIfInnerTextEquals("0");
            });
        }
    }
}