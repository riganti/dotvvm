using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class DependencyInjectionTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_DependencyInjection_ViewModelScopedService()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_DependencyInjection_ViewModelScopedService);

                for (int i = 0; i < 5; i++)
                {
                    var value = browser.First(".result").GetInnerText();
                    AssertUI.InnerTextEquals(browser.First(".result2"), value);

                    browser.First("input[type=button]").Click().Wait();
                    var value2 = browser.First(".result").GetInnerText();
                    AssertUI.InnerTextEquals(browser.First(".result2"), value2);

                    Assert.NotEqual(value, value2);
                }
            });
        }

        public DependencyInjectionTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
