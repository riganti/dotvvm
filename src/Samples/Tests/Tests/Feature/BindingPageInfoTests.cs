using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;

namespace DotVVM.Samples.Tests.Feature
{
    public class BindingPageInfoTests : AppSeleniumTest
    {
        public BindingPageInfoTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_BindingPageInfo_BindingPageInfo()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_BindingPageInfo_BindingPageInfo);

                var text = browser.Single("[data-ui=postback-text]");
                var button = browser.Single("[data-ui=long-postback-button]");

                AssertUI.InnerTextEquals(text, "no postback");
                button.Click();
                AssertUI.InnerTextEquals(text, "postback running");

                browser.Wait(1000);
                AssertUI.InnerTextEquals(text, "no postback");
            });
        }
    }
}
