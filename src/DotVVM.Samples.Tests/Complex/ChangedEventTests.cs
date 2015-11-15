using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class ChangedEventTests : SeleniumTestBase
    {
        [TestMethod]
        public void Complex_ChangedEvent()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ChangedEvent_ChangedEvent);

                browser.First("*[data-id='total-changes']").CheckIfInnerTextEquals("0");

                // first textbox with update mode on key press
                var textBox1 = browser.First("input[type=text]");
                new Actions(browser.Browser).Click(textBox1.WebElement).Perform();
                browser.Wait();
                new Actions(browser.Browser).SendKeys("test").Perform();

                browser.Wait();
                browser.First("*[data-id='total-changes']").CheckIfInnerTextEquals("0");
                browser.First("*[data-id='first-textbox']").CheckIfInnerTextEquals("Valuetest");

                new Actions(browser.Browser).SendKeys(Keys.Tab).Perform();
                browser.First("*[data-id='first-textbox']").CheckIfInnerTextEquals("Valuetest");
                browser.Wait();
                browser.First("*[data-id='total-changes']").CheckIfInnerTextEquals("1");

                // second textbox
                var textBox2 = browser.FindElements("input[type=text]")[1];
                new Actions(browser.Browser).Click(textBox2.WebElement).Perform();
                browser.Wait();
                new Actions(browser.Browser).SendKeys("test").Perform();

                browser.Wait();
                browser.First("*[data-id='total-changes']").CheckIfInnerTextEquals("1");
                browser.First("*[data-id='second-textbox']").CheckIfInnerTextEquals("Value");

                new Actions(browser.Browser).SendKeys(Keys.Tab).Perform();
                browser.First("*[data-id='second-textbox']").CheckIfInnerTextEquals("Valuetest");
                browser.Wait();
                browser.First("*[data-id='total-changes']").CheckIfInnerTextEquals("2");

                // click on checkbox
                browser.Click("input[type=checkbox]");
                browser.Wait();
                browser.First("*[data-id='total-changes']").CheckIfInnerTextEquals("3");

                browser.Click("input[type=checkbox]");
                browser.Wait();
                browser.First("*[data-id='total-changes']").CheckIfInnerTextEquals("4");

                // click on radio button
                browser.ElementAt("input[type=radio]", 0).Click().Wait();
                browser.First("*[data-id='total-changes']").CheckIfInnerTextEquals("5");

                browser.ElementAt("input[type=radio]", 1).Click().Wait();
                browser.First("*[data-id='total-changes']").CheckIfInnerTextEquals("6");

                browser.ElementAt("input[type=radio]", 2).Click().Wait();
                browser.First("*[data-id='total-changes']").CheckIfInnerTextEquals("7");

                browser.ElementAt("input[type=radio]", 3).Click().Wait();
                browser.First("*[data-id='total-changes']").CheckIfInnerTextEquals("8");

                browser.ElementAt("input[type=radio]", 4).Click().Wait();
                browser.First("*[data-id='total-changes']").CheckIfInnerTextEquals("9");

                // combo box
                browser.First("select").Select(1).Wait();
                browser.First("*[data-id='total-changes']").CheckIfInnerTextEquals("10");

                browser.First("select").Select(2).Wait();
                browser.First("*[data-id='total-changes']").CheckIfInnerTextEquals("11");

                browser.First("select").Select(0).Wait();
                browser.First("*[data-id='total-changes']").CheckIfInnerTextEquals("12");
            });
        }
    }
}