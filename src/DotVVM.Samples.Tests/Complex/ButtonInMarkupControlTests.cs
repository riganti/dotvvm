using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    public class ButtonInMarkupControlTests : AppSeleniumTest
    {
        [Fact]
        public void Complex_ButtonInMarkupControl_Enabled()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ButtonInMarkupControl_Enabled);
                browser.WaitUntilDotvvmInited();

                var enabled = browser.Single("enabled", SelectByDataUi);
                AssertUI.TextEquals(enabled, "false");
                browser.Single("btn-off", SelectByDataUi).Click();
                browser.WaitFor(() => AssertUI.TextEquals(enabled, "true"), 1000);
                browser.Single("btn-on", SelectByDataUi).Click();
                browser.WaitFor(() => AssertUI.TextEquals(enabled, "false"), 1000);
            });
        }

        public ButtonInMarkupControlTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
