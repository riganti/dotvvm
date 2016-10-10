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
    public class ViewModelProtectionTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_ViewModelProtection()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ViewModelProtection_ViewModelProtection);

                // get original value
                var originalValue = browser.First("strong span").GetText();

                // modify protected data
                browser.Last("a").Click();
                browser.Wait().Wait();

                // make sure it happened
                browser.First("strong span").CheckIfInnerTextEquals("hello");

                // try to do postback
                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait().Wait();

                // verify that the original value was restored
                browser.First("strong span").CheckIfInnerTextEquals(originalValue);
            });
        }
    }
}