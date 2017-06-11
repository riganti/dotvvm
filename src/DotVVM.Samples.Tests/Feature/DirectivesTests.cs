using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class DirectivesTests : SeleniumTest
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
    }
}