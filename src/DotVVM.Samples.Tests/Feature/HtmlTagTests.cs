using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class HtmlTagTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_HtmlTag_NonPairHtmlTag()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_HtmlTag_NonPairHtmlTag);

                browser.ElementAt("div", 0).FindElements("hr").ThrowIfDifferentCountThan(2);
                browser.ElementAt("div", 1).FindElements("hr").ThrowIfDifferentCountThan(1);

                AssertUI.InnerTextEquals(browser.ElementAt("div", 2).First("span"), "Hello");
            });
        }

        public HtmlTagTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
