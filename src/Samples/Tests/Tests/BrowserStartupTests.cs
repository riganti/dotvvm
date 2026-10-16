using DotVVM.Samples.Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests
{
    public class BrowserStartupTests : AppSeleniumTest
    {
        public BrowserStartupTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void BrowserStartup_CanActivateDefaultContent()
        {
            for (var i = 0; i < 2; i++)
            {
                RunInAllBrowsers(browser =>
                {
                    browser._GetInternalWebDriver().SwitchTo().DefaultContent();
                    browser.NavigateToUrl("about:blank");
                    browser._GetInternalWebDriver().SwitchTo().DefaultContent();
                });
            }
        }
    }
}
