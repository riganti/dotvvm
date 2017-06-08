using System.Linq;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class EnvironmentViewTests : SeleniumTest
    {
        [TestMethod]
        public void EnvironmentViewTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_EnvironmentView_EnvironmentViewTest);

                browser.FindElements(".result1").ThrowIfDifferentCountThan(1).First().CheckIfInnerTextEquals("Development or Production environment!");
                browser.FindElements(".result2").ThrowIfDifferentCountThan(1).First().CheckIfInnerTextEquals("Not Staging environment!");
            });
        }
    }
}