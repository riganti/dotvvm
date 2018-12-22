using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace DotVVM.Samples.Tests.New.Feature
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

                    Assert.AreNotEqual(value, value2);
                }
            });
        }

        public DependencyInjectionTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
