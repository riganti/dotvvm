using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class ValidationSummaryTests : AppSeleniumTest
    {
        [Fact]
        public void Control_ValidationSummary_RecursiveValidationSummary()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ValidationSummary_RecursiveValidationSummary);

                browser.ElementAt("input[type=button]", 0).Click().Wait();

                browser.ElementAt("ul", 0).FindElements("li").ThrowIfDifferentCountThan(2);
                AssertUI.InnerTextEquals(browser.First("#result"), "false");

                browser.ElementAt("input[type=button]", 1).Click().Wait();
                browser.ElementAt("ul", 1).FindElements("li").ThrowIfDifferentCountThan(1);
                AssertUI.InnerTextEquals(browser.First("#result"), "false");
            });
        }

        [Fact]
        public void Control_ValidationSummary_HideWhenValid()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ValidationSummary_HideWhenValid);

                AssertUI.IsNotDisplayed(browser.ElementAt("ul", 0));

                // Generate error
                browser.ElementAt("input[type=button]", 0).Click().Wait();
                AssertUI.IsDisplayed(browser.ElementAt("ul", 0));

                // Fix the error
                browser.SendKeys("input[type=text]", "message");
                browser.ElementAt("input[type=button]", 0).Click().Wait();
                AssertUI.IsNotDisplayed(browser.ElementAt("ul", 0));
            });
        }

        public ValidationSummaryTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
