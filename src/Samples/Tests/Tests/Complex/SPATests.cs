using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
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

                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_SPA_default);

                // go to test page
                AssertUI.TextEquals(browser.Single("h2"), "Default");
                browser.ElementAt("a", 1).Click();

                // check url and page contents
                AssertUI.TextEquals(browser.Single("h2"), "Test");
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPA/test"));

                // use the back button
                browser.NavigateBack();

                // check url and page contents
                AssertUI.TextEquals(browser.Single("h2"), "Default");
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPA/default"));

                // exit SPA using the back button
                browser.NavigateBack();

                // reenter SPA
                browser.NavigateForward();

                // check url and page contents
                AssertUI.TextEquals(browser.Single("h2"), "Default");
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPA/default"));

                // go forward to the test page
                browser.NavigateForward();

                // check url and page contents
                AssertUI.TextEquals(browser.Single("h2"), "Test");
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPA/test"));

                // open the default page
                browser.ElementAt("a", 0).Click();

                // check url and page contents
                AssertUI.TextEquals(browser.Single("h2"), "Default");
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPA/default"));

                // go back to the test page
                browser.NavigateBack();

                // check url and page contents
                AssertUI.TextEquals(browser.Single("h2"), "Test");
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPA/test"));

                // go back to the default page
                browser.NavigateBack();

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

                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_SPA_test);

                // click to generate validation error
                browser.Single("#validated-command").Click();

                // check if validation error is displayed
                AssertUI.InnerTextEquals(browser.Single("span[data-ui='sample-text']"), string.Empty);

                // go to default page
                browser.ElementAt("a", 0).Click();

                // click to check if validation error disapeared
                browser.Single("#validated-command").Click();
                browser.WaitForPostback();
                AssertUI.InnerTextEquals(browser.Single("span[data-ui='sample-text']"), "Sample Text");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPA_default))]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPA_test))]
        public void Complex_SPA_RedirectingLink()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl("/");


                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_SPA_default);

                // navigate to test
                browser.Single("#link-redirect").Click();

                AssertUI.UrlEquals(browser, browser.BaseUrl + SamplesRouteUrls.ComplexSamples_SPA_test);
                AssertUI.InnerTextEquals(browser.Single("h2"), "Test");

                // go to default page
                browser.NavigateBack();

                AssertUI.UrlEquals(browser, browser.BaseUrl + SamplesRouteUrls.ComplexSamples_SPA_default);
                AssertUI.InnerTextEquals(browser.Single("h2"), "Default");

                browser.NavigateForward();

                AssertUI.UrlEquals(browser, browser.BaseUrl + SamplesRouteUrls.ComplexSamples_SPA_test);
                AssertUI.InnerTextEquals(browser.Single("h2"), "Test");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPA_default))]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPA_test))]
        public void Complex_SPA_RedirectingCommand()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl("/");


                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_SPA_default);

                // navigate to test
                browser.Single("#button-redirect").Click();

                AssertUI.UrlEquals(browser, browser.BaseUrl + SamplesRouteUrls.ComplexSamples_SPA_test);
                AssertUI.InnerTextEquals(browser.Single("h2"), "Test");

                // go to default page
                browser.NavigateBack();

                AssertUI.UrlEquals(browser, browser.BaseUrl + SamplesRouteUrls.ComplexSamples_SPA_default);
                AssertUI.InnerTextEquals(browser.Single("h2"), "Default");

                browser.NavigateForward();

                AssertUI.UrlEquals(browser, browser.BaseUrl + SamplesRouteUrls.ComplexSamples_SPA_test);
                AssertUI.InnerTextEquals(browser.Single("h2"), "Test");
            });
        }

        public SPATests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
