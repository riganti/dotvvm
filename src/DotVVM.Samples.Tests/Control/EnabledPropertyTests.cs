using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Selenium.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using DotVVM.Testing.Abstractions;


namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class EnabledPropertyTests : AppSeleniumTest
    {
        [TestMethod]
        public void Control_EnabledProperty_EnabledProperty()
        {
            RunInAllBrowsers(browser =>
            {
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

                browser.ElementAt("label input[type=radio]", 0).Click();
                browser.ElementAt("label input[type=radio]", 1).Click();
                browser.ElementAt("label input[type=checkbox]", 0).Click();

                browser.ElementAt("label", 0).CheckIfIsNotSelected();
                browser.ElementAt("label", 1).CheckIfIsNotSelected();
                browser.ElementAt("label", 2).CheckIfIsNotSelected();
                browser.ElementAt("select", 1).CheckIfIsNotEnabled();
            });
        }
    }
}