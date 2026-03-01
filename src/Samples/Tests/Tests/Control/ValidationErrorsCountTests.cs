using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control;

public class ValidationErrorsCountTests : AppSeleniumTest
{
    public ValidationErrorsCountTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Control_ValidationErrorsCount_Basic()
    {
        RunInAllBrowsers(browser => {
            browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ValidationErrorsCount_Basic);

            var allValidation = browser.Single("validation-all", SelectByDataUi);
            var basicValidation = browser.Single("validation-basic", SelectByDataUi);
            var contactValidation = browser.Single("validation-contact", SelectByDataUi);
            var inputs = browser.FindElements("input[type=text]");
            var button = browser.Single("input[type=button]");

            // initialState
            ValidateBlock(allValidation, 0);
            ValidateBlock(basicValidation, 0);
            ValidateBlock(contactValidation, 0);

            // generate errors
            button.Click();
            ValidateBlock(allValidation, 4);
            ValidateBlock(basicValidation, 2);
            ValidateBlock(contactValidation, 2);

            // fix one error
            inputs[0].SendKeys("John");
            button.Click();
            ValidateBlock(allValidation, 3);
            ValidateBlock(basicValidation, 1);
            ValidateBlock(contactValidation, 2);

            // fix all errors
            inputs[1].SendKeys("Doe");
            inputs[2].SendKeys("123");
            inputs[3].SendKeys("test@mail.com");
            button.Click();

            ValidateBlock(allValidation, 0);
            ValidateBlock(basicValidation, 0);
            ValidateBlock(contactValidation, 0);


            void ValidateBlock(IElementWrapper element, int count)
            {
                browser.WaitFor(() => {
                    var counts = element.FindElements("span");
                    counts.ThrowIfDifferentCountThan(4);

                    AssertUI.TextEquals(counts[0], count.ToString());

                    AssertUI.TextEquals(counts[1], count.ToString());
                    if (count > 0)
                    {
                        AssertUI.HasClass(counts[1], "has-error");
                    }
                    else
                    {
                        AssertUI.HasNotClass(counts[1], "has-error");
                    }

                    if (count > 0)
                    {
                        AssertUI.TextEquals(counts[2], count.ToString());
                        AssertUI.IsDisplayed(counts[2]);
                    }
                    else
                    {
                        AssertUI.IsNotDisplayed(counts[2]);
                    }

                    AssertUI.TextEquals(counts[3], count == 0 ? "OK" : count == 1 ? "1 error" : $"{count} errors");
                }, 1000);
            }
        });
    }
}
