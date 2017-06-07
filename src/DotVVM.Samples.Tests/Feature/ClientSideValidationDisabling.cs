using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ClientSideValidationDisabling: SeleniumTest
    {
        [TestMethod]
        public void Feature_ClientSideValidationDisabling_ClientSideValidationDisabled()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ClientSideValidationDisabling);

                var requiredValidationResult = browser.Single("requiredValidationResult", SelectBy.Id);
                var emailValidationResult = browser.Single("emailValidationResult", SelectBy.Id);
                var validationTriggerButton = browser.First("input[type=button]");
                var requiredTextbox = browser.Single("required", SelectBy.Id);
                var emailTextbox = browser.Single("email", SelectBy.Id);

                requiredValidationResult.CheckIfIsNotDisplayed();
                emailValidationResult.CheckIfIsNotDisplayed();

                requiredTextbox.SendKeys("test");
                emailTextbox.SendKeys("test@test.test");
                validationTriggerButton.Click();
                
                requiredValidationResult.CheckIfIsNotDisplayed();
                emailValidationResult.CheckIfIsNotDisplayed();

                requiredTextbox.Clear();
                emailTextbox.Clear();
                emailTextbox.SendKeys("notEmail");
                validationTriggerButton.Click();

                requiredValidationResult.CheckIfIsDisplayed();
                emailValidationResult.CheckIfIsDisplayed();
            });
        }
        [TestMethod]
        public void Feature_ClientSideValidationDisabling_ClientSideValidationEnabled()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ClientSideValidationDisabling+"/true");

                var requiredValidationResult = browser.Single("requiredValidationResult", SelectBy.Id);
                var emailValidationResult = browser.Single("emailValidationResult", SelectBy.Id);
                var validationTriggerButton = browser.First("input[type=button]");
                var requiredTextbox = browser.Single("required", SelectBy.Id);
                var emailTextbox = browser.Single("email", SelectBy.Id);

                requiredValidationResult.CheckIfIsNotDisplayed();
                emailValidationResult.CheckIfIsNotDisplayed();

                requiredTextbox.SendKeys("test");
                emailTextbox.SendKeys("test@test.test");
                validationTriggerButton.Click();
                
                requiredValidationResult.CheckIfIsNotDisplayed();
                emailValidationResult.CheckIfIsNotDisplayed();

                requiredTextbox.Clear();
                emailTextbox.Clear();
                emailTextbox.SendKeys("notEmail");
                validationTriggerButton.Click();

                requiredValidationResult.CheckIfIsDisplayed();
                emailValidationResult.CheckIfIsNotDisplayed();
            });
        }
    }
}
