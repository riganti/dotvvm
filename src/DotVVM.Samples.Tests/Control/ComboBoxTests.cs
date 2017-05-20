using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.Selenium.Core;
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

        [TestMethod]
        public void Control_ComboBoxDelaySync()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ComboBoxDelaySync);

                // check that the second item is selected in both ComboBoxes on the page start
                browser.ElementAt("select", 0).ElementAt("option", 1).CheckIfIsSelected();
                browser.ElementAt("select", 1).ElementAt("option", 1).CheckIfIsSelected();

                // change the DataSource collection on the server and verify that the second item is selected in both ComboBoxes
                browser.First("input").Click().Wait();
                browser.ElementAt("select", 0).ElementAt("option", 1).CheckIfIsSelected();
                browser.ElementAt("select", 1).ElementAt("option", 1).CheckIfIsSelected();
            });
        }

        [TestMethod]
        public void Control_ComboBoxDelaySync2()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ComboBoxDelaySync2);
                browser.First("input[type=button]").Click().Wait();

                // check the comboboxes
                browser.ElementAt("select", 0).ElementAt("option", 0).CheckIfIsSelected();
                browser.ElementAt("select", 1).ElementAt("option", 1).CheckIfIsSelected();

                // check the labels
                browser.ElementAt(".result", 0).CheckIfInnerTextEquals("1");
                browser.ElementAt(".result", 1).CheckIfInnerTextEquals("2");
            });
        }
    }
}