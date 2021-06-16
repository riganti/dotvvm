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
        public void Feature_BindingVariables_Simple()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_BindingVariables_StaticCommandVariablesWithService_Simple);
                browser.WaitUntilDotvvmInited();
                browser.SelectMethod = SelectByDataUi;

                var button = browser.FirstOrDefault("get-messages");
                button.Click();

                AssertUI.TextEquals(browser.FirstOrDefault("message1"), "test1");
            });
        }

        [Fact]
        public void Feature_BindingVariables_ComplexObjectToObject()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_BindingVariables_StaticCommandVariablesWithService_Complex2);
                browser.WaitUntilDotvvmInited();
                browser.SelectMethod = SelectByDataUi;


                var button = browser.FirstOrDefault("get-messages");
                button.Click();

                AssertUI.TextEquals(browser.FirstOrDefault("message1"), "test1");
                AssertUI.TextEquals(browser.FirstOrDefault("message2"), "test2");

            });
        }
        [Fact]
        public void Feature_BindingVariables_ComplexPropsToProps()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_BindingVariables_StaticCommandVariablesWithService_Complex);
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
