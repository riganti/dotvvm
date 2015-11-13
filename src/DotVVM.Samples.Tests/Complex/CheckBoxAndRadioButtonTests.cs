using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class CheckBoxAndRadioButtonTests : SeleniumTestBase
    {
        [TestMethod]
        public void Complex_CheckBoxAndRadioButton()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("ComplexSamples/CheckBoxAndRadioButton/CheckBoxAndRadioButton");

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

                // radion button list
                boxes.ElementAt(2).ElementAt("input[type=radio]", 2).Click();
                boxes.ElementAt(2).ElementAt("input[type=radio]", 3).Click();
                boxes.ElementAt(2).First("input[type=button]").Click();
                browser.Wait();

                boxes.ElementAt(2).Last("span")
                    .CheckIfInnerTextEquals("4");
                
                boxes.ElementAt(2).ElementAt("input[type=radio]", 1).Click();
                boxes.ElementAt(2).First("input[type=button]").Click();
                browser.Wait();

                boxes.ElementAt(2).Last("span")
                    .CheckIfInnerTextEquals("2");

                // checked changed
                boxes.ElementAt(3).ElementAt("input[type=checkbox]", 0).Click();
                browser.Wait();

                boxes.ElementAt(3).Last("span")
                    .CheckIfInnerTextEquals("1");
                boxes.ElementAt(3).First("input[type=checkbox]")
                    .CheckIfIsChecked();

                boxes.ElementAt(3).ElementAt("input[type=checkbox]", 0).Click();
                browser.Wait();

                boxes.ElementAt(3).Last("span")
                   .CheckIfInnerTextEquals("2");
                boxes.ElementAt(3).First("input[type=checkbox]")
                    .CheckIfIsNotChecked();
            });
        }
    }
}