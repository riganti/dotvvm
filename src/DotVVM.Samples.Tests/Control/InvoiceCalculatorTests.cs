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
    public class InvoiceCalculatorTests : SeleniumTestBase
    {
        [TestMethod]
        public void InvoiceCalculatorTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("ControlSamples/InvoiceCalculator");
                var table = browser.First(".table");
                var addButton = browser.ElementAt("a", 1);
                var recalculateButton = browser.ElementAt("a", 2);

                // add lines
                addButton.Click();
                browser.Wait();
                addButton.Click();
                browser.Wait();
                addButton.Click();
                browser.Wait();

                // fill second line
                var cells = table.FindElements("tr")[2].FindElements("td");
                cells[0].First("input").Clear().SendKeys("111");
                cells[1].First("select").Select(1);
                cells[2].First("input").Clear().SendKeys("Bread");
                cells[3].First("input").Clear().SendKeys("12");
                cells[4].First("input").Clear().SendKeys("10");

                // fill third line
                cells = table.FindElements("tr")[3].FindElements("td");
                cells[0].First("input").Clear().SendKeys("222");
                cells[1].First("select").Select(2);
                cells[2].First("input").Clear().SendKeys("Ham");
                cells[3].First("input").Clear().SendKeys("1");
                cells[4].First("input").Clear().SendKeys("5");

                // fill fourth line
                cells = table.FindElements("tr")[4].FindElements("td");
                cells[0].First("input").Clear().SendKeys("333");
                cells[1].First("select").Select(3);
                cells[2].First("input").Clear().SendKeys("Cheese");
                cells[3].First("input").Clear().SendKeys("10");
                cells[4].First("input").Clear().SendKeys("15");

                // verify line totals
                browser.First("input[type=text]").Click();
                table.FindElements("tr")[2].FindElements("td")[5].CheckIfInnerTextEquals("126");
                table.FindElements("tr")[3].FindElements("td")[5].CheckIfInnerTextEquals("5.5");
                table.FindElements("tr")[4].FindElements("td")[5].CheckIfInnerTextEquals("180");

                // recalculate
                recalculateButton.Click().Wait();

                // verify total price
                table.Last("tr").FindElements("th")[1].CheckIfInnerTextEquals("407.5");

                // remove second line
                table.FindElements("tr")[2].Last("td").First("a").Click().Wait();

                // verify total price
                table.Last("tr").FindElements("th")[1].CheckIfInnerTextEquals("281.5");
            });
        }
    }
}