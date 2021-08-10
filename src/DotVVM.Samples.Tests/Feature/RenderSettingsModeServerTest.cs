using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class RenderSettingsModeServerTest : AppSeleniumTest
    {
        [Fact]
        public void Feature_RenderSettingsModeServer_RenderSettingModeServerProperty()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_RenderSettingsModeServer_RenderSettingModeServerProperty);

                // ensure month names are rendered on the server
                browser.FindElements("table tr td span").ThrowIfDifferentCountThan(12);

                // fill textboxes
                browser.SendKeys("input[type=text]", "1");

                browser.Click("input[type=button]");

                // validate result
                AssertUI.InnerTextEquals(browser.Last("span"), "12", false, true);
            });
        }

        public RenderSettingsModeServerTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}
