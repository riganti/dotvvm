using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using DotVVM.Testing.Abstractions;


namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class CheckBoxTests : AppSeleniumTest
    {
        [TestMethod]
        public void Control_CheckBox_CheckBox()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_CheckBox);

                var boxes = browser.FindElements("fieldset");

                // single check box
                boxes.ElementAt(0).First("input[type=checkbox]").Click();
                boxes.ElementAt(0).First("input[type=button]").Click();
                browser.Wait();

                boxes.ElementAt(0).First("span.result")
                    .CheckIfInnerTextEquals("True");

                // check box list
                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 1).Click();
                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 2).Click();
                boxes.ElementAt(1).First("input[type=button]").Click();
                browser.Wait();

                boxes.ElementAt(1).First("span.result")
                    .CheckIfInnerTextEquals("g, b");

                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 2).Click();
                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 0).Click();
                boxes.ElementAt(1).First("input[type=button]").Click();
                browser.Wait();

                boxes.ElementAt(1).First("span.result")
                    .CheckIfInnerTextEquals("g, r");

                // checked changed
                boxes.ElementAt(2).ElementAt("input[type=checkbox]", 0).Click();
                browser.Wait();

                boxes.ElementAt(2).Last("span.result")
                    .CheckIfInnerTextEquals("1");
                boxes.ElementAt(2).First("input[type=checkbox]")
                    .CheckIfIsChecked();

                boxes.ElementAt(2).ElementAt("input[type=checkbox]", 0).Click();
                browser.Wait();

                boxes.ElementAt(2).Last("span.result")
                    .CheckIfInnerTextEquals("2");
                boxes.ElementAt(2).First("input[type=checkbox]")
                    .CheckIfIsNotChecked();

                // checked visible
                var v = boxes.ElementAt(4);
                boxes.ElementAt(4).ElementAt("input[type=checkbox]", 0).CheckIfIsDisplayed();
                boxes.ElementAt(4).ElementAt("input[type=checkbox]", 1).CheckIfIsNotDisplayed();

                boxes.ElementAt(4).Single("input[data-ui=switch]").Click();

                boxes.ElementAt(4).ElementAt("input[type=checkbox]", 0).CheckIfIsNotDisplayed();
                boxes.ElementAt(4).ElementAt("input[type=checkbox]", 1).CheckIfIsDisplayed();

                // dataContext change
                boxes.ElementAt(5).First("input[type=checkbox]").Click();
                boxes.ElementAt(5).First("span.result")
                    .CheckIfInnerTextEquals("true");
            });
        }

        [TestMethod]
        public void Control_CheckBox_InRepeater()
        {
            RunInAllBrowsers(browser =>
            {
                
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_InRepeater);

                var repeater = browser.Single("div[data-ui='repeater']");
                var checkBoxes = browser.FindElements("label[data-ui='checkBox']");

                var checkBox = checkBoxes.ElementAt(0).Single("input");
                checkBox.Click();
                checkBox.CheckIfIsChecked();
                browser.Single("span[data-ui='selectedColors']")
                .CheckIfInnerText(s => s.Contains("orange"));

                checkBox = checkBoxes.ElementAt(1).Single("input");
                checkBox.Click();
                checkBox.CheckIfIsChecked();
                browser.Single("span[data-ui='selectedColors']")
                .CheckIfInnerText(s => s.Contains("orange") && s.Contains("red"));

                checkBox = checkBoxes.ElementAt(2).Single("input");
                checkBox.Click();
                checkBox.CheckIfIsChecked();
                browser.Single("span[data-ui='selectedColors']")
                .CheckIfInnerText(s => s.Contains("orange") && s.Contains("red") && s.Contains("black"));

                checkBoxes = browser.FindElements("label[data-ui='checkBox']");

                browser.First("[data-ui='set-server-values']").Click();
                checkBoxes.ElementAt(0).Single("input").CheckIfIsChecked();
                checkBoxes.ElementAt(2).Single("input").CheckIfIsChecked();
                browser.Single("span[data-ui='selectedColors']")
                .CheckIfInnerText(s => s.Contains("orange") && s.Contains("black"));
            });
        }

        //TODO: check this test
        //[TestMethod]
        //public void Control_CheckBox_NullCollection()
        //{
        //    RunInAllBrowsers(browser =>
        //    {
        //        browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_CheckedItemsNull);
        //    });
        //}

        [TestMethod]
        public void Control_CheckBox_Indeterminate()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_Indeterminate);

                var checkBox = browser.First("input[type=checkbox]");
                var reset = browser.First("input[type=button]");
                var value = browser.First("span.value");

                value.CheckIfTextEquals("Indeterminate");
                checkBox.Click();
                value.CheckIfTextEquals("Other");
                reset.Click();
                value.CheckIfTextEquals("Indeterminate");
            });
        }
    }
}