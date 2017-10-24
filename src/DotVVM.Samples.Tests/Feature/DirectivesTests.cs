using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class DirectivesTests : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_Directives_ViewModelMissingAssembly()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Directives_ViewModelMissingAssembly);
                browser.FindElements("#failed").ThrowIfDifferentCountThan(0);
            });
        }

        [TestMethod]
        public void Feature_Directives_ImportDirectiveInvalid()
        {
            

            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Directives_ImportDirectiveInvalid);
                browser.FindElements("#failed").ThrowIfDifferentCountThan(0);
            });
        }

        [TestMethod]
        public void Feature_Directives_ImportDirective()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Directives_ImportDirective);

                // check all textxs from resources
                browser.ElementAt("p", 0).CheckIfInnerTextEquals("Hello from ImportDirectiveViewModel");
                browser.ElementAt("p", 1).CheckIfInnerTextEquals("Hello TestClass1");
                browser.ElementAt("p", 2).CheckIfInnerTextEquals("Hello TestClassNonAlias");
                browser.ElementAt("p", 3).CheckIfInnerTextEquals("Default from configuration"); // maybe more posibilities?
            });
        }
    }
}