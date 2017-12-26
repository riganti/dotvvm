using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Selenium.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using DotVVM.Samples.Tests.New;
using DotVVM.Testing.Abstractions;
using Xunit;
using Xunit.Abstractions;


namespace DotVVM.Samples.Tests.Control
{
    public class ComboBoxTests : AppSeleniumTest
    {
        public ComboBoxTests(ITestOutputHelper output) : base(output)
        {
        }
        [Fact]
        public void Control_ComboBox_ComboBox()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ComboBox);

                AssertUI.IsDisplayed(browser.First("select").Select(0));
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(browser.First("span"), "1");
                }, 2000, 30);

                // select second option from combobox
                browser.First("select").Select(1);
                browser.WaitFor(() => { AssertUI.InnerTextEquals(browser.First("span"), "2"); }, 1000, 30);

                // select third option from combobox
                browser.First("select").Select(2);
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(browser.First("span"), "3"); ;
                }, 1000, 30);

                // select fourth option from combobox
                browser.First("select").Select(3);
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(browser.First("span"), "4"); ;
                }, 1000, 30);
            });
        }

        [Fact]
        public void Control_ComboBox_ComboBoxDelaySync()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ComboBoxDelaySync);

                // check that the second item is selected in both ComboBoxes on the page start
                AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 1));
                AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 1));

                // change the DataSource collection on the server and verify that the second item is selected in both ComboBoxes
                browser.First("input").Click();

                browser.WaitFor(() => {
                    AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 1));
                    AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 1));
                }, 800, 30);
            });
        }

        [Fact]
        public void Control_ComboBox_ComboBoxDelaySync2()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ComboBoxDelaySync2);
                browser.First("input[type=button]").Click();

                browser.WaitFor(() => {


                    // check the comboboxes
                    AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 0));
                    AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 1));

                    // check the labels
                    AssertUI.InnerTextEquals(browser.ElementAt(".result", 0), "1");
                    AssertUI.InnerTextEquals(browser.ElementAt(".result", 1), "2");
                }, 1000, 30);
            });
        }


        [Fact]
        public void Control_ComboBox_ComboBoxDelaySync3()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ComboBoxDelaySync3);
                browser.First("input[type=button]").Click();

                browser.WaitFor(() => {
                // check that the combobox appears
                    AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 0));
                }, 1000, 30);
            });
        }


    }
}
