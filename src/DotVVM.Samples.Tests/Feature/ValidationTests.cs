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
    public class ValidationTests: SeleniumTest
    {
        [TestMethod]
        public void Feature_Validation_ClientSideValidationDisabling()
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
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_Validation_ClientSideValidationDisabling))]
        public void Feature_Validation_ClientSideValidationDisabling_Enabled()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ClientSideValidationDisabling + "/true");

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

        [TestMethod]
        public void Feature_Validation_DynamicValidation()
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

        [TestMethod]
        public void Feature_Validation_NestedValidation()
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
                browser.ElementAt("span", 1).CheckIfInnerTextEquals("25");

                browser.ElementAt("input", 0).SendKeys("a");
                browser.Wait();
                browser.ElementAt("input[type=button]", 0).Click();

                browser.ElementAt("span", 0).CheckIfIsDisplayed();
                browser.ElementAt("span", 1).CheckIfInnerTextEquals("25");
            });
        }

        [TestMethod]
        public void Feature_Validation_SimpleValidation()
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

        /// <summary>
        /// Feature_s the validation rules load on postback.
        /// </summary>
        [TestMethod]
        [Timeout(120000)]
        public void Feature_Validation_ValidationRulesLoadOnPostback()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ValidationRulesLoadOnPostback);

                // click the validate button
                browser.Last("input[type=button]").Click();
                browser.Wait();

                // ensure validators are hidden
                browser.Last("span").CheckIfInnerTextEquals("true");
                browser.FindElements("li").ThrowIfDifferentCountThan(0);
                // load the customer
                browser.Click("input[type=button]");

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

        [TestMethod]
        public void Feature_Validation_ValidationScopes()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ValidationScopes);

                browser.First("input[type=button]").Click();

                browser.First("li").CheckIfInnerText(i => i.Contains("The Value field is required."));
            });
        }

        [TestMethod]
        public void Feature_Validation_ValidationScopes2()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ValidationScopes2);

                // we are testing the first button

                // don't fill required field, the client validation should appear
                browser.Single(".result").CheckIfTextEquals("0");
                browser.ElementAt("input[type=button]", 0).Click();
                browser.Single(".result").CheckIfTextEquals("0");
                browser.ElementAt("input[type=text]", 0).CheckIfHasClass("has-error");
                browser.ElementAt("input[type=text]", 1).CheckIfHasNotClass("has-error");

                // fill first required field and second field with a short string, the server validation should appear
                browser.ElementAt("input[type=text]", 0).SendKeys("aaa");
                browser.ElementAt("input[type=text]", 1).SendKeys("aaa");
                browser.Single(".result").CheckIfTextEquals("0");
                browser.ElementAt("input[type=button]", 0).Click();
                browser.Single(".result").CheckIfTextEquals("0");
                browser.ElementAt("input[type=text]", 0).CheckIfHasNotClass("has-error");
                browser.ElementAt("input[type=text]", 1).CheckIfHasClass("has-error");

                // fill the second field so the validation passes
                browser.ElementAt("input[type=text]", 1).Clear().SendKeys("aaaaaa");
                browser.Single(".result").CheckIfTextEquals("0");
                browser.ElementAt("input[type=button]", 0).Click();
                browser.Single(".result").CheckIfTextEquals("1");
                browser.ElementAt("input[type=text]", 0).CheckIfHasNotClass("has-error");
                browser.ElementAt("input[type=text]", 1).CheckIfHasNotClass("has-error");

                // clear the fields
                browser.ElementAt("input[type=text]", 0).Clear();
                browser.ElementAt("input[type=text]", 1).Clear();

                // we are testing the second button

                // don't fill required field, the client validation should appear
                browser.Single(".result").CheckIfTextEquals("1");
                browser.ElementAt("input[type=button]", 1).Click();
                browser.Single(".result").CheckIfTextEquals("1");
                browser.ElementAt("input[type=text]", 0).CheckIfHasClass("has-error");
                browser.ElementAt("input[type=text]", 1).CheckIfHasNotClass("has-error");

                // fill first required field and second field with a short string, the server validation should appear
                browser.ElementAt("input[type=text]", 0).SendKeys("aaa");
                browser.ElementAt("input[type=text]", 1).SendKeys("aaa");
                browser.Single(".result").CheckIfTextEquals("1");
                browser.ElementAt("input[type=button]", 1).Click();
                browser.Single(".result").CheckIfTextEquals("1");
                browser.ElementAt("input[type=text]", 0).CheckIfHasNotClass("has-error");
                browser.ElementAt("input[type=text]", 1).CheckIfHasClass("has-error");

                // fill the second field so the validation passes
                browser.ElementAt("input[type=text]", 1).Clear().SendKeys("aaaaaa");
                browser.Single(".result").CheckIfTextEquals("1");
                browser.ElementAt("input[type=button]", 1).Click();
                browser.Single(".result").CheckIfTextEquals("2");
                browser.ElementAt("input[type=text]", 0).CheckIfHasNotClass("has-error");
                browser.ElementAt("input[type=text]", 1).CheckIfHasNotClass("has-error");

            });
        }
    }
}
