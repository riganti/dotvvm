using System.Linq;
using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Selenium.Core;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ChildViewModelInvokeMethodsTests : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_ChildViewModelInvokeMethods_ChildViewModelInvokeMethods()
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

        private static void CheckIfInnerTextEqualsToOne(IBrowserWrapperFluentApi browser, string dataUi)
        {
            browser.FindElements($"[data-ui='{dataUi}']").First().CheckIfInnerTextEquals(1.ToString());
        }
    }
}