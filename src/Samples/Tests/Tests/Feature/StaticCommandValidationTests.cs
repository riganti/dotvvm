using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class StaticCommandValidationTests : AppSeleniumTest
    {
        public StaticCommandValidationTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Theory]
        [InlineData("btn-validate-text", "input-text")]
        [InlineData("btn-validate-username", "input-username")]
        [InlineData("btn-validate-username-this", "input-username")]
        [InlineData("btn-validate-text-parent", "input-text")]
        [InlineData("btn-validate-text-root", "input-text")]
        public void Feature_StaticCommandValidation_ServerSide_AddArgumentError_DifferentWaysToPassArguments(string buttonDataUi, string inputDataUi)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_Validation);
                var btnValidate = browser.Single(buttonDataUi, SelectByDataUi);
                var inputText = browser.Single(inputDataUi, SelectByDataUi);
                var validationSummary = browser.First("[data-ui=validation-summary]");

                btnValidate.Click();
                browser.WaitForPostback();
                AssertUI.HasClass(inputText, "has-error");
                AssertUI.TextEquals(validationSummary, "Input can not be null or empty");

                inputText.SendKeys("TestString");
                btnValidate.Click();
                browser.WaitForPostback();
                AssertUI.HasNotClass(inputText, "has-error");
                AssertUI.TextEquals(validationSummary, "");
            });
        }

        [Fact]
        public void Feature_StaticCommandValidation_ServerSide_AddArgumentError_CustomPropertyPath()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_Validation);
                var btnAddError = browser.Single("btn-add-error", SelectByDataUi);
                var propertyPathText = browser.Single("input-propertypath", SelectByDataUi);
                var errorMessageText = browser.Single("input-errormessage", SelectByDataUi);
                var validationSummary = browser.First("[data-ui=validation-summary]");

                const string errorMessage = "Enter a valid user name!";
                propertyPathText.Clear().SendKeys("/User/Name");
                errorMessageText.Clear().SendKeys(errorMessage);
                btnAddError.Click();
                browser.WaitForPostback();
                AssertUI.TextEquals(validationSummary, errorMessage);
            });
        }
    }
}
