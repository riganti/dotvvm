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
    public class CheckBoxTests : SeleniumTest
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
        public void Control_CheckBox_CheckboxInRepeater()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_CheckboxInRepeater);

                browser.Single("#checkbox-one").Click();
                browser.Single("#checkbox-one").CheckIfIsChecked();
                browser.First("span").CheckIfInnerText(s => s.Contains("one"));

                browser.Single("#checkbox-two").Click();
                browser.Single("#checkbox-two").CheckIfIsChecked();
                browser.First("span").CheckIfInnerText(s => s.Contains("one") && s.Contains("two"));

                browser.Single("#checkbox-three").Click();
                browser.Single("#checkbox-three").CheckIfIsChecked();
                browser.First("span").CheckIfInnerText(s => s.Contains("one") && s.Contains("two") && s.Contains("three"));

                browser.First("#set-server-values").Click();
                browser.Single("#checkbox-one").CheckIfIsChecked();
                browser.Single("#checkbox-three").CheckIfIsChecked();
                browser.First("span").CheckIfInnerText(s => s.Contains("one") && s.Contains("three"));
            });
        }
    }
}