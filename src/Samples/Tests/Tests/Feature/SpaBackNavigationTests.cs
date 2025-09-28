using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class SpaBackNavigationTests : AppSeleniumTest
    {
        public SpaBackNavigationTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_SpaBackNavigation_Page1))]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_SpaBackNavigation_Page2))]
        public void Feature_SpaBackNavigation_Test()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl("/");

                // enter SPA page 1
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_SpaBackNavigation_Page1);

                // go to page 2
                browser.ElementAt("a", 1).Click();

                // check url and page contents
                AssertUI.TextEquals(browser.Single("h2"), "Page2");
                AssertUI.Url(browser, s => s.Contains("FeatureSamples/SpaBackNavigation/Page2"));

                // go to PDF document
                browser.ElementAt("a", 2).Click();
                browser.Wait(2000);

                // go back to SPA
                browser.NavigateBack();

                // check the SPA loaded correctly - typeid must be from Page2
                AssertUI.Text(browser.Single("#viewmodel"), t => t.Contains("\"JlGkqjB61pYLbneg\": {"));

                // bindings must work
                AssertUI.TextEquals(browser.Single("test-binding", SelectByDataUi), "Bindings work!");
            });
        }
    }
}
