using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
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
                browser.Wait(1000);

                //check url
                AssertUI.Url(browser, s => s.Contains("/ComplexSamples/SPARedirect/login?ReturnUrl=%2FComplexSamples%2FSPARedirect"));

                // login to the app
                var loginElement = browser.First("input[type=button]");
                AssertUI.Attribute(loginElement, "value", "Login");
                loginElement.Click().Wait(1000);

                //check url
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPARedirect"));

                // sign out
                AssertUI.Attribute(loginElement, "value", "Sign Out");
                loginElement.Click().Wait(1000);

                //check url
                AssertUI.Url(browser, s => s.Contains("/ComplexSamples/SPARedirect/login?ReturnUrl=%2FComplexSamples%2FSPARedirect"));

                // login to the app
                AssertUI.Attribute(loginElement, "value", "Login");
                loginElement.Click().Wait(1000);

                //check url
                AssertUI.Url(browser, s => s.Contains("ComplexSamples/SPARedirect"));

                // sign out
                AssertUI.Attribute(loginElement, "value", "Sign Out");
                loginElement.Click().Wait(1000);
            });
        }

        public SPARedirectTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
