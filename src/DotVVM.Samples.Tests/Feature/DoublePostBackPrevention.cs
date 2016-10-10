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
    public class DoublePostBackPreventionTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_DoublePostBackPrevention()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_DoublePostBackPrevention_DoublePostBackPrevention);

                // try the long action interrupted by the short one
                browser.ElementAt("input[type=button]", 0).Click();
                browser.Wait(2000);
                browser.ElementAt("input[type=button]", 1).Click();

                // the postback index should be 1 now (because of short action)
                browser.ElementAt("span", 0).CheckIfInnerTextEquals("1");
                browser.ElementAt("span", 1).CheckIfInnerTextEquals("short");
                
                // the result of the long action should be canceled, the counter shouldn't increase
                browser.Wait(10000);
                browser.ElementAt("span", 0).CheckIfInnerTextEquals("1");
                browser.ElementAt("span", 1).CheckIfInnerTextEquals("short");
                browser.Wait();

                // test update progress control
                browser.CheckIfIsNotDisplayed("div[data-bind='dotvvm-UpdateProgress-Visible: true']");
                browser.ElementAt("input[type=button]", 2).Click();
                browser.CheckIfIsDisplayed("div[data-bind='dotvvm-UpdateProgress-Visible: true']");
                browser.Wait(6000);
                browser.CheckIfIsNotDisplayed("div[data-bind='dotvvm-UpdateProgress-Visible: true']");
            });
        }
    }
}