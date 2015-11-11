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
    public class CheckBoxAndRadioButtonTests : SeleniumTestBase
    {
        [TestMethod]
        public void CheckBoxAndRadioButtonTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("ControlSamples/CheckBoxAndRadioButton");

                var boxes = browser.FindElements("fieldset");
                
                // single check box
                boxes[0].First("input[type=checkbox]").Click();
                boxes[0].First("input[type=button]").Click();
                browser.Wait();

                boxes[0].First("span")
                    .CheckIfInnerTextEquals("True");

                // check box list
                boxes[1].FindElements("input[type=checkbox]")[1].Click();
                boxes[1].FindElements("input[type=checkbox]")[2].Click();
                boxes[1].First("input[type=button]").Click();
                browser.Wait();

                boxes[1].First("span")
                    .CheckIfInnerTextEquals("g, b");

                boxes[1].FindElements("input[type=checkbox]")[2].Click();
                boxes[1].FindElements("input[type=checkbox]")[0].Click();
                boxes[1].First("input[type=button]").Click();
                browser.Wait();

                boxes[1].First("span")
                    .CheckIfInnerTextEquals("g, r");

                // radion button list
                boxes[2].FindElements("input[type=radio]")[2].Click();
                boxes[2].FindElements("input[type=radio]")[3].Click();
                boxes[2].First("input[type=button]").Click();
                browser.Wait();

                boxes[2].Last("span")
                    .CheckIfInnerTextEquals("4");

                boxes[2].FindElements("input[type=radio]")[1].Click();
                boxes[2].First("input[type=button]").Click();
                browser.Wait();

                boxes[2].Last("span")
                    .CheckIfInnerTextEquals("2");

                // checked changed
                boxes[3].FindElements("input[type=checkbox]")[0].Click();
                browser.Wait();

                boxes[3].Last("span")
                    .CheckIfInnerTextEquals("1");
                boxes[3].First("input[type=checkbox]")
                    .CheckIfIsChecked();

                boxes[3].FindElements("input[type=checkbox]")[0].Click();
                browser.Wait();

                boxes[3].Last("span")
                   .CheckIfInnerTextEquals("2");
                boxes[3].First("input[type=checkbox]")
                    .CheckIfIsNotChecked();
            });
        }
    }
}