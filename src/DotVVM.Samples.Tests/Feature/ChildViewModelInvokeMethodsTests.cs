using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class ChildViewModelInvokeMethodsTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_ChildViewModelInvokeMethods_ChildViewModelInvokeMethods()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ChildViewModelInvokeMethods_ChildViewModelInvokeMethods);

                CheckIfInnerTextEqualsToOne(browser, "InitCount");
                CheckIfInnerTextEqualsToOne(browser, "LoadCount");
                CheckIfInnerTextEqualsToOne(browser, "PreRenderCount");

                CheckIfInnerTextEqualsToOne(browser, "NastyInitCount");
                CheckIfInnerTextEqualsToOne(browser, "NastyLoadCount");
                CheckIfInnerTextEqualsToOne(browser, "NastyPreRenderCount");
            });
        }

        private static void CheckIfInnerTextEqualsToOne(IBrowserWrapper browser, string dataUi)
        {
            AssertUI.InnerTextEquals(browser.FindElements($"[data-ui='{dataUi}']").First(), 1.ToString());
        }

        public ChildViewModelInvokeMethodsTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
