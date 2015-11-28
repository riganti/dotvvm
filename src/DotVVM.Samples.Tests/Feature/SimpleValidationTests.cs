using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class SimpleValidationTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_SimpleValidation()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_SimpleValidation);

                // ensure validators not visible
                browser.FindElements("li").ThrowIfDifferentCountThan(0);

                browser.ElementAt("span", 0).CheckIfIsNotDisplayed();
                browser.ElementAt("span", 1).CheckIfIsDisplayed();
                browser.ElementAt("span", 1).CheckClassAttribute(s => !s.Contains("validator"));
                browser.ElementAt("span", 2).CheckIfIsNotDisplayed();

                // leave textbox empty and submit the form
                browser.Click("input[type=button]");

                // ensure validators visible
                browser.FindElements("li").ThrowIfDifferentCountThan(1);

                browser.ElementAt("span", 0).CheckIfIsDisplayed();
                browser.ElementAt("span", 1).CheckIfIsDisplayed();
                browser.ElementAt("span", 1).CheckClassAttribute(s => s.Contains("validator"));
                browser.ElementAt("span", 2).CheckIfIsDisplayed();

                // submit once again and test the validation summary still holds one error
                browser.Click("input[type=button]");
                browser.FindElements("li").ThrowIfDifferentCountThan(1);

                // fill invalid value in the task title
                browser.SendKeys("input[type=text]", "test");
                browser.Wait();
                browser.Click("input[type=button]");

                // ensure validators visible
                browser.FindElements("li").ThrowIfDifferentCountThan(1);

                browser.ElementAt("span", 0).CheckIfIsDisplayed();
                browser.ElementAt("span", 1).CheckIfIsDisplayed();
                browser.ElementAt("span", 1).CheckClassAttribute(s => s.Contains("validator"));
                browser.ElementAt("span", 2).CheckIfIsDisplayed();

                // fill valid value in the task title
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "test@mail.com");
                browser.Wait();
                browser.Click("input[type=button]");

                // ensure validators not visible
                browser.FindElements("li").ThrowIfDifferentCountThan(0);

                browser.ElementAt("span", 0).CheckIfIsNotDisplayed();
                browser.ElementAt("span", 1).CheckIfIsDisplayed();
                browser.ElementAt("span", 1).CheckClassAttribute(s => !s.Contains("validator"));
                browser.ElementAt("span", 2).CheckIfIsNotDisplayed();

                // ensure the item was added
                browser.FindElements(".table tr").ThrowIfDifferentCountThan(4);
            });
        }
    }
}