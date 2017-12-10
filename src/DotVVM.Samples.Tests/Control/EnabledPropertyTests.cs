using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class EnabledPropertyTests : AppSeleniumTest
    {
        [TestMethod]
        public void Control_EnabledProperty_EnabledProperty()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_EnabledProperty_EnabledProperty);

                browser.ElementAt("select", 0).CheckIfIsEnabled();
                browser.ElementAt("input", 0).CheckIfIsEnabled();
                browser.ElementAt("label", 0).CheckIfIsEnabled();
                browser.ElementAt("label", 1).CheckIfIsEnabled();
                browser.ElementAt("label", 2).CheckIfIsEnabled();
                browser.ElementAt("select", 1).CheckIfIsEnabled();

                browser.First("input[type=button]").Click().Wait();

                browser.ElementAt("select", 0).CheckIfIsNotEnabled();
                browser.ElementAt("input", 0).CheckIfIsNotEnabled();

                try
                {
                    browser.ElementAt("label input[type=radio]", 0).Click();
                    browser.ElementAt("label input[type=radio]", 1).Click();
                    browser.ElementAt("label input[type=checkbox]", 0).Click();
                }
                catch (InvalidElementStateException ex) when (ex.Message == "Element is not enabled")
                {
                    // NOOP
                }

                browser.ElementAt("label", 0).CheckIfIsNotSelected();
                browser.ElementAt("label", 1).CheckIfIsNotSelected();
                browser.ElementAt("label", 2).CheckIfIsNotSelected();
                browser.ElementAt("select", 1).CheckIfIsNotEnabled();
            });
        }
    }
}
