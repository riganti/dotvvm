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
                IElementWrapper getLoginElement() => browser.First("input[type=button]");
                AssertUI.Attribute(getLoginElement(), "value", "Login");
                getLoginElement().Click();

                //check url
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPARedirect"));

                // sign out
                AssertUI.Attribute(getLoginElement(), "value", "Sign Out");
                getLoginElement().Click();

                //check url
                AssertUI.Url(browser, s => s.Contains("/ComplexSamples/SPARedirect/login?ReturnUrl=%2FComplexSamples%2FSPARedirect"));

                // login to the app
                AssertUI.Attribute(getLoginElement(), "value", "Login");
                getLoginElement().Click();

                //check url
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPARedirect"));

                // sign out
                AssertUI.Attribute(getLoginElement(), "value", "Sign Out");
                getLoginElement().Click();
            });
        }

        public SPARedirectTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
