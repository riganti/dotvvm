using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class ConditionalCssClass : AppSeleniumTest
    {
        [Fact]
        public void Feature_ConditionalCssClasses_ConditionalCssClasses()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ConditionalCssClasses_ConditionalCssClasses);

                AssertUI.HasNotClass(browser.First("div"), "italic");
                browser.First("input[type=button][value=\"Switch Italic\"]").Click();
                AssertUI.HasClass(browser.First("div"), "italic");

                AssertUI.HasNotClass(browser.First("div"), "bordered");
                browser.First("input[type=button][value=\"Switch Bordered\"]").Click();
                AssertUI.HasClass(browser.First("div"), "bordered");

                AssertUI.HasNotClass(browser.First("div"), "blue");
                browser.First("input[type=button][value=\"Switch Blue\"]").Click();
                AssertUI.HasClass(browser.First("div"), "blue");
            });
        }

        public ConditionalCssClass(ITestOutputHelper output) : base(output)
        {
        }
    }
}
