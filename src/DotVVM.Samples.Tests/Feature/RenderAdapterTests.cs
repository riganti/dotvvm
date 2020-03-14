using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class RenderAdapterTests : AppSeleniumTest
    {
        public RenderAdapterTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_RenderAdapter_Basic()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_RenderAdapter_Basic);

                var standard = browser.Single("standard", By.Id);

                AssertUI.TagName(standard, "input");
                AssertUI.HasNotAttribute(standard, "test");
                AssertUI.InnerTextEquals(standard, "TEXT");


                var replaced = browser.Single("replaced", By.Id);
                AssertUI.TagName(replaced, "div");
                AssertUI.HasAttribute(replaced, "test");
                AssertUI.InnerTextEquals(replaced, "REPLACEMENT TEXT");
            });
        }
    }
}
