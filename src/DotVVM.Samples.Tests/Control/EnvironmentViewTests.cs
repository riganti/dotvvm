using System.Linq;
using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Selenium.Core;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class EnvironmentViewTests : AppSeleniumTest
    {
        [TestMethod]
        public void Control_EnvironmentView_EnvironmentViewTest()
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