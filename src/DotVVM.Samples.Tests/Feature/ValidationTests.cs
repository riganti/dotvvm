using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ValidationTests : SeleniumTest
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
        public void Feature_Validation_DateTimeValidation()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_DateTimeValidation);

                var textBox = browser.First("input[type=text]");
                var button = browser.First("input[type=button]");

                // empty field - should have error
                textBox.Clear();
                button.Click();
                textBox.CheckIfHasClass("has-error");

                // corrent value - no error
                textBox.SendKeys("06/14/2017 8:10:35 AM");
                browser.Wait(2000);
                button.Click();
                textBox.CheckIfHasNotClass("has-error");

                // incorrent value - should have error
                textBox.Clear();
                textBox.SendKeys("06-14-2017");
                button.Click();
                textBox.CheckIfHasClass("has-error");

                // correct value - no error
                textBox.Clear();
                textBox.SendKeys("10/13/2017 10:30:50 PM");
                button.Click();
                textBox.CheckIfHasNotClass("has-error");
            });
        }

        [TestMethod]
        public void Feature_Validation_DateTimeValidation_NullableDateTime()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_DateTimeValidation_NullableDateTime);
                var textBox1 = browser.ElementAt("input[type=text]", 0);
                var textBox2 = browser.ElementAt("input[type=text]", 1);
                var button = browser.Single("input[type=button]");
                var errorField = browser.Single(".validation-error");

                // empty field - no error
                textBox1.Clear();
                button.Click();
                textBox1.CheckIfHasNotClass("has-error");
                textBox2.CheckIfHasNotClass("has-error");
                errorField.CheckIfIsNotDisplayed();

                // invalid value - should report error
                textBox1.SendKeys("06-14-2017");
                button.Click();
                textBox1.CheckIfHasClass("has-error");
                textBox2.CheckIfHasClass("has-error");
                errorField.CheckIfIsDisplayed();

                // valid value - no error
                textBox1.Clear();
                textBox1.SendKeys(DateTime.Now.ToString("dd/MM/yyyy H:mm:ss", CultureInfo.InvariantCulture));
                button.Click();
                textBox1.CheckIfHasNotClass("has-error");
                textBox2.CheckIfHasNotClass("has-error");
                errorField.CheckIfIsNotDisplayed();
                textBox1.CheckIfInnerTextEquals(textBox2.GetInnerText());

                // one textbox has invalid value and second gets valid - should have no error
                textBox1.Clear();
                textBox1.SendKeys("Invalid value");
                textBox2.SendKeys(DateTime.Now.ToString("dd/MM/yyyy H:mm:ss", CultureInfo.InvariantCulture));
                button.Click();
                textBox1.CheckIfHasNotClass("has-error");
                textBox2.CheckIfHasNotClass("has-error");
                errorField.CheckIfIsNotDisplayed();
                textBox1.CheckIfInnerTextEquals(textBox2.GetInnerText());
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
        public void Feature_Validation_EssentialTypeValidation()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_EssentialTypeValidation);

                var addNestedBtn = browser.ElementAt("input[type=button]", 0);
                var withBtn = browser.ElementAt("input[type=button]", 1);
                var withOutBtn = browser.ElementAt("input[type=button]", 2);

                // withnout nested test
                browser.FindElements("li").ThrowIfDifferentCountThan(0);
                withOutBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(0);
                withBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                browser.First("li").CheckIfInnerTextEquals("The NullableIntegerProperty field is required.");
                withOutBtn.Click();                                         // should not remove the validation error
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                browser.First(".nullableInt input[type=text]").SendKeys("5");
                withOutBtn.Click();                                         // should not remove the validation error
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                withBtn.Click();                                            // should remove the validation error
                browser.FindElements("li").ThrowIfDifferentCountThan(0);

                // with nested test
                browser.First(".nullableInt input[type=text]").Clear();
                addNestedBtn.Click();
                withOutBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(0);
                withBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(4);
                browser.ElementAt(".nullableInt input[type=text]", 0).SendKeys("10");
                browser.ElementAt(".nullableInt input[type=text]", 2).SendKeys("10");
                withOutBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(4);
                withBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(2);

                // wrong value test
                browser.ElementAt(".nullableInt input[type=text]", 3).SendKeys("15");
                browser.First(".NaNTest input[type=text]").SendKeys("asd");
                withBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(2);
                browser.First("li")
                    .CheckIfInnerTextEquals(
                        "The value of property NullableFloatProperty (asd) is invalid value for type double?.");

                // correct form test
                browser.First(".NaNTest input[type=text]").Clear();
                browser.First(".NaNTest input[type=text]").SendKeys("55");
                browser.ElementAt(".nullableInt input[type=text]", 1).SendKeys("15");
                withOutBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(2);
                withBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(0);
            });
        }

        [TestMethod]
        public void Feature_Validation_ModelStateErrors()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ModelStateErrors);

                //click first button - viewmodel error
                browser.ElementAt("input[type=button]", 0).Click();
                browser.FindElements(".vmErrors li").ThrowIfDifferentCountThan(1);
                browser.ElementAt(".vm1Error", 0).CheckIfIsNotDisplayed();
                browser.ElementAt(".vm2Error", 0).CheckIfIsNotDisplayed();
                browser.ElementAt(".vm2Error", 1).CheckIfIsNotDisplayed();
                browser.ElementAt(".vm2Error", 2).CheckIfIsNotDisplayed();

                //click second button - nested viewmodel1 error
                browser.ElementAt("input[type=button]", 1).Click();
                browser.FindElements(".vmErrors li").ThrowIfDifferentCountThan(1);
                browser.ElementAt(".vm1Error", 0).CheckIfIsDisplayed();
                browser.ElementAt(".vm2Error", 0).CheckIfIsNotDisplayed();
                browser.ElementAt(".vm2Error", 1).CheckIfIsNotDisplayed();
                browser.ElementAt(".vm2Error", 2).CheckIfIsNotDisplayed();

                //click third button - nested viewmodel2 two errors
                browser.ElementAt("input[type=button]", 2).Click();
                browser.FindElements(".vmErrors li").ThrowIfDifferentCountThan(2);
                browser.ElementAt(".vm1Error", 0).CheckIfIsNotDisplayed();
                browser.ElementAt(".vm2Error", 0).CheckIfIsDisplayed();
                browser.ElementAt(".vm2Error", 1).CheckIfIsNotDisplayed();
                browser.ElementAt(".vm2Error", 2).CheckIfIsDisplayed();
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
        public void Feature_Validation_NullValidationTarget()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_NullValidationTarget);

                //get buttons
                var targetRootBtn = browser.ElementAt("input[type=button]", 0);
                var targetNullBtn = browser.ElementAt("input[type=button]", 1);
                var targetSomeBtn = browser.ElementAt("input[type=button]", 2);

                //test both fields empty
                targetRootBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(2);
                browser.ElementAt("li", 0).CheckIfInnerTextEquals("The NullObject field is required.");
                browser.ElementAt("li", 1).CheckIfInnerTextEquals("The Required field is required.");

                targetNullBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(0);

                targetSomeBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                browser.First("li").CheckIfInnerTextEquals("The Required field is required.");

                //test invalid Email and empty Required
                browser.ElementAt("input[type=text]", 0).SendKeys("invalid");

                targetRootBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(2);
                browser.ElementAt("li", 0).CheckIfInnerTextEquals("The NullObject field is required.");
                browser.ElementAt("li", 1).CheckIfInnerTextEquals("The Required field is required.");

                targetNullBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(0);

                // The invalid Email won't be reported because emails are checked only on the server
                targetSomeBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                browser.ElementAt("li", 0).CheckIfInnerTextEquals("The Required field is required.");

                //test valid Email and empty Required
                browser.ElementAt("input[type=text]", 0).Clear();
                browser.ElementAt("input[type=text]", 0).SendKeys("valid@test.com");

                targetRootBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(2);
                browser.ElementAt("li", 0).CheckIfInnerTextEquals("The NullObject field is required.");
                browser.ElementAt("li", 1).CheckIfInnerTextEquals("The Required field is required.");

                targetNullBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(0);

                targetSomeBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                browser.First("li").CheckIfInnerTextEquals("The Required field is required.");

                //test invalid Email and filled Required
                browser.ElementAt("input[type=text]", 0).Clear();
                browser.ElementAt("input[type=text]", 0).SendKeys("invalid");
                browser.ElementAt("input[type=text]", 1).SendKeys("filled");

                targetRootBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                browser.ElementAt("li", 0).CheckIfInnerTextEquals("The NullObject field is required.");

                targetNullBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(0);

                // The invalid email will be reported this time because now the check makes it to the server
                targetSomeBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                browser.ElementAt("li", 0).CheckIfInnerTextEquals("The Email field is not a valid e-mail address.");

                //test valid Email and filled Required (valid form - expect for null object)
                browser.ElementAt("input[type=text]", 0).Clear();
                browser.ElementAt("input[type=text]", 0).SendKeys("valid@test.com");

                targetRootBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                browser.ElementAt("li", 0).CheckIfInnerTextEquals("The NullObject field is required.");

                targetNullBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(0);

                targetSomeBtn.Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(0);
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
