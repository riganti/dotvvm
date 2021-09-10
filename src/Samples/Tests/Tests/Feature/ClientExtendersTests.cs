using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class ClientExtendersTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_ClientExtenders_PasswordStrength()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ClientExtenders_PasswordStrength);

                var message = browser.Single("[data-ui='strength-message']");
                AssertUI.TextEquals(message, "Enter password");

                var textBox = browser.Single("[data-ui='password-textBox']");
                textBox.SendKeys("password");

                AssertUI.TextEquals(message, "Good");
            });
        }

        public ClientExtendersTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
