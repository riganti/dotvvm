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
    public class DynamicValidationTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_DynamicValidation()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_DynamicValidation);

                // click the validate button
                browser.Last("input[type=button]").Click();

                // ensure validators are hidden
                browser.Last("span").CheckIfInnerTextEquals("true");
                browser.FindElements("li").ThrowIfDifferentCountThan(0);

                // load the customer
                browser.Click("input[type=button]");
                browser.Wait();

                // try to validate
                browser.Last("input[type=button]").Click();

                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                browser.First("li").CheckIfInnerText(s => s.Contains("Email"));

                // fix the e-mail address
                browser.Last("input[type=text]").Clear();
                browser.Last("input[type=text]").SendKeys("test@mail.com");
                browser.Last("input[type=button]").Click();

                // try to validate
                browser.Last("span").CheckIfInnerTextEquals("true");
                browser.FindElements("li").ThrowIfDifferentCountThan(0);
            });
        }
    }
}