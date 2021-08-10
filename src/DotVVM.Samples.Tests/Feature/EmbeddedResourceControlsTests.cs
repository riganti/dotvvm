using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class EmbeddedResourceControlsTests : AppSeleniumTest
    {
        public EmbeddedResourceControlsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_EmbeddedResourceControls_EmbeddedResourceControls()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_EmbeddedResourceControls_EmbeddedResourceControls);

                AssertUI.Attribute(browser.First("input[type=button]"), "value", "Nothing");

                browser.First("input[type=button]").Click();

                AssertUI.Attribute(browser.First("input[type=button]"), "value", "This is text");
            });
        }
    }
}
