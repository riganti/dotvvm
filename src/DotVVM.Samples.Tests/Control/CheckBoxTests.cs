using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class CheckBoxTests : SeleniumTestBase
    {
        [TestMethod]
        public void Control_CheckBox()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("ControlSamples/CheckBox/CheckBox");

                var boxes = browser.FindElements("fieldset");
                
                // single check box
                boxes.ElementAt(0).First("input[type=checkbox]").Click();
                boxes.ElementAt(0).First("input[type=button]").Click();
                browser.Wait();

                boxes.ElementAt(0).First("span")
                    .CheckIfInnerTextEquals("True");

                // check box list
                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 1).Click();
                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 2).Click();
                boxes.ElementAt(1).First("input[type=button]").Click();
                browser.Wait();

                boxes.ElementAt(1).First("span")
                    .CheckIfInnerTextEquals("g, b");

                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 2).Click();
                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 0).Click();
                boxes.ElementAt(1).First("input[type=button]").Click();
                browser.Wait();

                boxes.ElementAt(1).First("span")
                    .CheckIfInnerTextEquals("g, r");

                // checked changed
                boxes.ElementAt(2).ElementAt("input[type=checkbox]", 0).Click();
                browser.Wait();

                boxes.ElementAt(2).Last("span")
                    .CheckIfInnerTextEquals("1");
                boxes.ElementAt(2).First("input[type=checkbox]")
                    .CheckIfIsChecked();

                boxes.ElementAt(2).ElementAt("input[type=checkbox]", 0).Click();
                browser.Wait();

                boxes.ElementAt(2).Last("span")
                   .CheckIfInnerTextEquals("2");
                boxes.ElementAt(2).First("input[type=checkbox]")
                    .CheckIfIsNotChecked();
            });
        }
    }
}