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
    public class NestedValidationTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_NestedValidation()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_NestedValidation);

                // ensure validators not visible
                browser.ElementAt("span", 0).CheckIfIsNotDisplayed();
                browser.ElementAt("span", 1).CheckIfIsDisplayed();
                browser.ElementAt("span", 1).CheckClassAttribute(s => !s.Contains("validator"));
                browser.ElementAt("span", 2).CheckIfIsNotDisplayed();

                browser.FindElements(".summary1 li").ThrowIfDifferentCountThan(0);
                browser.FindElements(".summary2 li").ThrowIfDifferentCountThan(0);
                
                // leave textbox empty and submit the form
                browser.Click("input[type=button]");

                // ensure validators visible
                browser.ElementAt("span", 0).CheckIfIsDisplayed();
                browser.ElementAt("span", 1).CheckIfIsDisplayed();
                browser.ElementAt("span", 1).CheckClassAttribute(s => s.Contains("invalid"));
                browser.ElementAt("span", 2).CheckIfIsDisplayed();

                browser.FindElements(".summary1 li").ThrowIfDifferentCountThan(0);
                browser.FindElements(".summary2 li").ThrowIfDifferentCountThan(1);

                // submit once again and test the validation summary still holds one error
                browser.Click("input[type=button]");

                // ensure validators visible
                browser.ElementAt("span", 0).CheckIfIsDisplayed();
                browser.ElementAt("span", 1).CheckIfIsDisplayed();
                browser.ElementAt("span", 1).CheckClassAttribute(s => s.Contains("invalid"));
                browser.ElementAt("span", 2).CheckIfIsDisplayed();

                browser.FindElements(".summary1 li").ThrowIfDifferentCountThan(0);
                browser.FindElements(".summary2 li").ThrowIfDifferentCountThan(1);

                // fill invalid value in the task title
                browser.SendKeys("input[type=text]", "test");
                browser.Wait();
                browser.Click("input[type=button]");

                // ensure validators
                browser.ElementAt("span", 0).CheckIfIsNotDisplayed();
                browser.ElementAt("span", 1).CheckIfIsDisplayed();
                browser.ElementAt("span", 1).CheckClassAttribute(s => !s.Contains("validator"));
                browser.ElementAt("span", 2).CheckIfIsNotDisplayed();

                browser.FindElements(".summary1 li").ThrowIfDifferentCountThan(0);
                browser.FindElements(".summary2 li").ThrowIfDifferentCountThan(0);

            });
        }
    }
}