using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class ResourcesTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_Resources_CdnUnavailableResourceLoad()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Resources_CdnUnavailableResourceLoad);

                // verify that if CDN is not available, local script loads
                browser.WaitFor(browser.HasAlert, 5000, "An alert was expected to open!");
                AssertUI.AlertTextEquals(browser, "javascript resource loaded!");
                browser.ConfirmAlert();
            });
        }

        [Fact]
        public void Feature_Resources_CdnScriptPriority()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Resources_CdnScriptPriority);

                // verify that if CDN is not available, local script loads
                browser.WaitFor(browser.HasAlert, 5000, "An alert was expected to open!");
                AssertUI.AlertTextEquals(browser, "javascript resource loaded!");
                browser.ConfirmAlert();
            });
        }

        [Fact]
        public void Feature_Resources_OnlineNonameResourceLoad()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Resources_OnlineNonameResourceLoad);

                //click buton
                browser.First("input[type=button]").Click();

                //check that alert showed
                browser.WaitFor(browser.HasAlert, 5000, "An alert was expected to open!");
                AssertUI.AlertTextEquals(browser, "resource loaded");
                browser.ConfirmAlert();
            });
        }

        [Fact]
        public void Feature_Resources_LocationFallback()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Resources_LocationFallback);
                browser.WaitUntilDotvvmInited();

                AssertUI.TextEquals(browser.Single("h1:nth-of-type(2)", SelectBy.CssSelector),
                    "The \"testResource_locationFallback\" script has been loaded.");
            });
        }

        public ResourcesTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
