using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Selenium.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using DotVVM.Testing.Abstractions;


namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class RegexValidationTests : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_Validation_RegexValidation()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_RegexValidation);

                browser.ElementAt("input", 0).SendKeys("25");
                browser.Wait();
                browser.ElementAt("input[type=button]", 0).Click();

                browser.ElementAt("span", 0).CheckIfIsNotDisplayed();
                browser.Wait();
                browser.ElementAt("span", 1).CheckIfInnerTextEquals("25");

                browser.ElementAt("input", 0).SendKeys("a");
                browser.Wait();
                browser.ElementAt("input[type=button]", 0).Click();

                browser.ElementAt("span", 0).CheckIfIsDisplayed();
                browser.ElementAt("span", 1).CheckIfInnerTextEquals("25");
            });
        }
    }
}