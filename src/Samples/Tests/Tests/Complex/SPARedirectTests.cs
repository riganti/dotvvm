using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    public class SPARedirectTests : AppSeleniumTest
    {
        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPARedirect_home))]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPARedirect_login))]
        public void Complex_SPARedirect_RedirectToLoginPage()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl("ComplexSamples/SPARedirect");

                //check url
                AssertUI.Url(browser, s => s.Contains("/ComplexSamples/SPARedirect/login?ReturnUrl=%2FComplexSamples%2FSPARedirect"));

                // login to the app
                IElementWrapper getLoginElement(string dataUi) => browser.First("input[type=button][data-ui='" + dataUi + "']");
                AssertUI.Attribute(getLoginElement("login"), "value", "Login");
                getLoginElement("login").Click();

                browser.WaitFor(() => {
                    //check url
                    AssertUI.Url(browser, s => s.EndsWith("ComplexSamples/SPARedirect"), waitForOptions: WaitForOptions.Disabled);
                    AssertUI.Attribute(getLoginElement("signout"), "value", "Sign Out", waitForOptions: WaitForOptions.Disabled);
                }, 2_000);
                // sign out
                getLoginElement("signout").Click();

                browser.WaitFor(() => {
                    //check url
                    AssertUI.Url(browser, s => s.Contains("/ComplexSamples/SPARedirect/login?ReturnUrl=%2FComplexSamples%2FSPARedirect"), waitForOptions: WaitForOptions.Disabled);

                    AssertUI.Attribute(getLoginElement("login"), "value", "Login", waitForOptions: WaitForOptions.Disabled);
                }, 2_000);
                // login to the app
                getLoginElement("login").Click();

                //check url
                browser.WaitFor(() => {
                    //check url
                    AssertUI.Url(browser, s => s.EndsWith("ComplexSamples/SPARedirect"), waitForOptions: WaitForOptions.Disabled);
                    AssertUI.Attribute(getLoginElement("signout"), "value", "Sign Out", waitForOptions: WaitForOptions.Disabled);
                }, 2_000);
                // sign out
                getLoginElement("signout").Click();
            });
        }

        public SPARedirectTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
