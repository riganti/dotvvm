using DotVVM.Samples.Tests.Base;
using DotVVM.Samples.Tests.Complex;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class BindingVariablesTests : AppSeleniumTest
    {
        public BindingVariablesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_BindingPageInfo_BindingPageInfo()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_BindingVariables_StaticCommandVariablesWithService);
                browser.WaitUntilDotvvmInited();
                browser.SelectMethod = SelectByDataUi;


                var button = browser.FirstOrDefault("get-messages");
                button.Click();

                AssertUI.TextEquals(browser.FirstOrDefault("message1"), "test1");
                AssertUI.TextEquals(browser.FirstOrDefault("message2"), "test2");

            });
        }
    }
}
