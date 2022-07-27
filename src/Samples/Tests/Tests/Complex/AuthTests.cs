using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    public class AuthTests : AppSeleniumTest
    {
        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_Auth_SecuredPage))]
        [Trait("Category", "dev-only")] // relies on error page
        public void Complex_Auth_Login()
        {
            RunInAllBrowsers(browser => {
                // try to visit the secured page and verify we are redirected
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_Auth_SecuredPage);
                AssertUI.Url(browser, u => u.Contains(SamplesRouteUrls.ComplexSamples_Auth_Login));

                // use the login page
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_Auth_Login);

                browser.SendKeys("input[type=text]", "user");
                browser.First("input[type=button]").Click();
                browser.Refresh();
                browser.Last("a").Click();

                browser.SendKeys("input[type=text]", "message");
                browser.First("input[type=button]").Click();

                AssertUI.InnerText(browser.ElementAt("h1", 1),
                        s =>
                            s.Contains("DotVVM Debugger: Error 403: Forbidden"),
                            "User is not in admin role"
                        );

                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_Auth_Login);

                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "ADMIN");
                browser.First("input[type=checkbox]").Click();
                browser.First("input[type=button]").Click();
                browser.Last("a").Click();

                browser.SendKeys("input[type=text]", "message");
                browser.First("input[type=button]").Click();

                AssertUI.InnerText(browser.First("span"), s => s.Contains("ADMIN: message"), "User can't send message");
            });
        }

        public AuthTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
