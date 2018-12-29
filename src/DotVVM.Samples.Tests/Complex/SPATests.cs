using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    public class SPATests : AppSeleniumTest
    {
        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPA_default))]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPA_test))]
        public void Complex_SPA_NavigationAndBackButtons()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl("/");
                browser.Wait(1000);

                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_SPA_default);
                browser.Wait(1000);

                // go to test page
                AssertUI.TextEquals(browser.Single("h2"), "Default");
                browser.ElementAt("a", 1).Click().Wait();

                // check url and page contents
                AssertUI.TextEquals(browser.Single("h2"), "Test");
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPA/test"));

                // use the back button
                browser.NavigateBack();
                browser.Wait(1000);

                // check url and page contents
                AssertUI.TextEquals(browser.Single("h2"), "Default");
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPA/default"));

                // exit SPA using the back button
                browser.NavigateBack();
                browser.Wait(1000);

                // reenter SPA
                browser.NavigateForward();
                browser.Wait(1000);

                // check url and page contents
                AssertUI.TextEquals(browser.Single("h2"), "Default");
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPA/default"));

                // go forward to the test page
                browser.NavigateForward();
                browser.Wait(1000);

                // check url and page contents
                AssertUI.TextEquals(browser.Single("h2"), "Test");
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPA/test"));

                // open the default page
                browser.ElementAt("a", 0).Click().Wait();

                // check url and page contents
                AssertUI.TextEquals(browser.Single("h2"), "Default");
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPA/default"));

                // go back to the test page
                browser.NavigateBack();
                browser.Wait(1000);

                // check url and page contents
                AssertUI.TextEquals(browser.Single("h2"), "Test");
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPA/test"));

                // go back to the default page
                browser.NavigateBack();
                browser.Wait(1000);

                // check url and page contents
                AssertUI.TextEquals(browser.Single("h2"), "Default");
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPA/default"));
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPA_default))]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPA_test))]
        public void Complex_SPA_ValidationAndNavigation()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl("/");
                browser.Wait(1000);

                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_SPA_test);
                browser.Wait(1000);

                // click to generate validation error
                browser.Single("input[type=button]").Click();

                // check if validation error is displayed
                browser.Wait(500);
                AssertUI.InnerTextEquals(browser.Single("span[data-ui='sample-text']"), string.Empty);

                // go to default page
                browser.ElementAt("a", 0).Click().Wait();
                browser.Wait(1000);

                // click to check if validation error disapeared
                browser.Single("input[type=button]").Click();
                browser.Wait(500);
                AssertUI.InnerTextEquals(browser.Single("span[data-ui='sample-text']"), "Sample Text");
            });
        }

        public SPATests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
