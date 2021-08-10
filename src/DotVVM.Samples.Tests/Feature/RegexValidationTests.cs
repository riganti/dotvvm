using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class RegexValidationTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_Validation_RegexValidation()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_RegexValidation);

                browser.ElementAt("input", 0).SendKeys("25");
                browser.ElementAt("input[type=button]", 0).Click();

                AssertUI.IsNotDisplayed(browser.ElementAt("span", 0));
                AssertUI.InnerTextEquals(browser.ElementAt("span", 1), "25");

                browser.ElementAt("input", 0).SendKeys("a");
                browser.ElementAt("input[type=button]", 0).Click();

                AssertUI.IsDisplayed(browser.ElementAt("span", 0));
                AssertUI.InnerTextEquals(browser.ElementAt("span", 1), "25");
            });
        }

        public RegexValidationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
