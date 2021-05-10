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
                AssertUI.TextEquals(enabled, "true");
                browser.Single("btn-on", SelectByDataUi).Click();
                AssertUI.TextEquals(enabled, "false");
            });
        }

        public ButtonInMarkupControlTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
