using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class DirectivesTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_Directives_ViewModelMissingAssembly()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Directives_ViewModelMissingAssembly);
                browser.FindElements("#failed").ThrowIfDifferentCountThan(0);
            });
        }

        [Fact]
        public void Feature_Directives_ImportDirectiveInvalid()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Directives_ImportDirectiveInvalid);
                browser.FindElements("#failed").ThrowIfDifferentCountThan(0);
            });
        }

        [Fact]
        public void Feature_Directives_ImportDirective()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Directives_ImportDirective);

                // check all texts from resources
                AssertUI.InnerTextEquals(browser.ElementAt("p", 0), "Hello from ImportDirectiveViewModel");
                AssertUI.InnerTextEquals(browser.ElementAt("p", 1), "Hello TestClass1");
                AssertUI.InnerTextEquals(browser.ElementAt("p", 2), "Hello TestClassNonAlias");
                AssertUI.InnerTextEquals(browser.ElementAt("p", 3), "Default from configuration");
                AssertUI.InnerTextEquals(browser.ElementAt("p", 4), "Hello From Nested Class"); // maybe more possibilities?
            });
        }

        public DirectivesTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
