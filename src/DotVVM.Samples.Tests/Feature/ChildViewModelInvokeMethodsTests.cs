using System.Linq;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ChildViewModelInvokeMethodsTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_ChildViewModelInvokeMethodsTests()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ChildViewModelInvokeMethods_ChildViewModelInvokeMethods);

                CheckIfInnerTextEqualsToOne(browser, "InitCount");
                CheckIfInnerTextEqualsToOne(browser, "LoadCount");
                CheckIfInnerTextEqualsToOne(browser, "PreRenderCount");

                CheckIfInnerTextEqualsToOne(browser, "NastyInitCount");
                CheckIfInnerTextEqualsToOne(browser, "NastyLoadCount");
                CheckIfInnerTextEqualsToOne(browser, "NastyPreRenderCount");
            });
        }

        private static void CheckIfInnerTextEqualsToOne(BrowserWrapper browser, string dataUi)
        {
            browser.FindElements($"[data-ui='{dataUi}']").First().CheckIfInnerTextEquals(1.ToString());
        }
    }
}