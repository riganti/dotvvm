using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class ValidationSummaryTests : SeleniumTestBase
    {

        [TestMethod]
        public void Control_ValidationSummary_RecursiveValidationSummary()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ValidationSummary_RecursiveValidationSummary);

                browser.ElementAt("input[type=button]", 0).Click().Wait();

                browser.ElementAt("ul", 0).FindElements("li").ThrowIfDifferentCountThan(2);
                browser.First("#result").CheckIfInnerTextEquals("false");
                
                browser.ElementAt("input[type=button]", 1).Click().Wait();
                browser.ElementAt("ul", 1).FindElements("li").ThrowIfDifferentCountThan(1);
                browser.First("#result").CheckIfInnerTextEquals("false");
            });
        }

        [TestMethod]
        public void Control_ValidationSummary_IncludeErrorsFromTarget_PropertyPathNull()
        {
            Control_ValidationSummary_IncludeErrorsFromTarget(SamplesRouteUrls.ControlSamples_ValidationSummary_IncludeErrorsFromTarget_PropertyPathNull);
        }

        [TestMethod]
        public void Control_ValidationSummary_IncludeErrorsFromTarget_PropertyPathNotNull()
        {
            Control_ValidationSummary_IncludeErrorsFromTarget(SamplesRouteUrls.ControlSamples_ValidationSummary_IncludeErrorsFromTarget_PropertyPathNotNull, true);
        }

        private void Control_ValidationSummary_IncludeErrorsFromTarget(string url, bool clickButton = false)
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(url);

                if(clickButton)
                {
                    browser.Single("setup-button", this.SelectByDataUi).Click();
                    browser.Wait();
                }

                var loginButton = browser.Single("login-button", this.SelectByDataUi);
                loginButton.Click();
                browser.Wait();

                CheckValidationSummary(browser, "The Nick field is required.", "The Password field is required.");

                var nickTextbox = browser.Single("nick-textbox", this.SelectByDataUi);
                nickTextbox.SendKeys("Mike");

                var passwordTextbox = browser.Single("password-textbox", this.SelectByDataUi);
                passwordTextbox.SendKeys("123");

                browser.FireJsBlur();
                loginButton.Click();
                browser.Wait();

                CheckValidationSummary(browser, "Wrong Nick or Password.");

                passwordTextbox.SendKeys("4");

                browser.FireJsBlur();
                loginButton.Click();
                browser.Wait();

                browser.Single("logout-button", this.SelectByDataUi).CheckIfIsDisplayed();

            });
        }

        private void CheckValidationSummary(BrowserWrapper browser, params string[] errors)
        {
            var validationSummary = browser.Single("validationSummary", this.SelectByDataUi);
            Assert.AreEqual(errors.Length, validationSummary.Children.Count);
            for (int i = 0; i < errors.Length; i++)
            {
                validationSummary.ElementAt("li", i).CheckIfTextEquals(errors[i]);

            }
        }

    }
}