using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class ComboBoxTests : SeleniumTestBase
    {
        [TestMethod]
        public void Control_ComboBox()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ComboBox);

                browser.First("select").Select(0).CheckIfIsDisplayed().Wait();
                browser.First("span").CheckIfInnerTextEquals("1");

                // select second option from combobox
                browser.First("select").Select(1).Wait();
                browser.First("span").CheckIfInnerTextEquals("2");

                // select third option from combobox
                browser.First("select").Select(2).Wait();
                browser.First("span").CheckIfInnerTextEquals("3");

                // select fourth option from combobox
                browser.First("select").Select(3).Wait();
                browser.First("span").CheckIfInnerTextEquals("4");
            });
        }
    }
}