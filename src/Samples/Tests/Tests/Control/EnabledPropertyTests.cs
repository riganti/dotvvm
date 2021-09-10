using System.Configuration;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class EnabledPropertyTests : AppSeleniumTest
    {
        [Fact]
        public void Control_EnabledProperty_EnabledProperty()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_EnabledProperty_EnabledProperty);

                AssertUI.IsEnabled(browser.ElementAt("select", 0));
                AssertUI.IsEnabled(browser.ElementAt("input", 0));
                AssertUI.IsEnabled(browser.ElementAt("label", 0));
                AssertUI.IsEnabled(browser.ElementAt("label", 1));
                AssertUI.IsEnabled(browser.ElementAt("label", 2));
                AssertUI.IsEnabled(browser.ElementAt("select", 1));
                AssertUI.IsEnabled(browser.First("[data-ui=button]"));

                browser.First("[data-ui=switch-button]").Click();

                AssertUI.IsNotEnabled(browser.ElementAt("select", 0));
                AssertUI.IsNotEnabled(browser.ElementAt("input", 0));

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

                AssertUI.IsNotSelected(browser.ElementAt("label", 0));
                AssertUI.IsNotSelected(browser.ElementAt("label", 1));
                AssertUI.IsNotSelected(browser.ElementAt("label", 2));
                AssertUI.IsNotEnabled(browser.ElementAt("select", 1));
                AssertUI.IsNotEnabled(browser.First("[data-ui=button]"));
            });
        }

        public EnabledPropertyTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
