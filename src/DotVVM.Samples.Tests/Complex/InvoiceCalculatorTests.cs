using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    public class InvoiceCalculatorTests : AppSeleniumTest
    {
        [Fact]
        public void Complex_InvoiceCalculator_InvoiceCalculator()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_InvoiceCalculator_InvoiceCalculator);

                var table = browser.First(".table");
                var addButton = browser.ElementAt("a", 1);
                var recalculateButton = browser.ElementAt("a", 2);

                // add lines
                addButton.Click();
                
                addButton.Click();
                
                addButton.Click();
                

                // fill second line
                var cells = table.ElementAt("tr", 2).FindElements("td");
                cells.ElementAt(0).First("input").Clear().SendKeys("111");
                cells.ElementAt(1).First("select").Select(1);
                cells.ElementAt(2).First("input").Clear().SendKeys("Bread");
                cells.ElementAt(3).First("input").Clear().SendKeys("12");
                cells.ElementAt(4).First("input").Clear().SendKeys("10");

                // fill third line
                cells = table.ElementAt("tr", 3).FindElements("td");
                cells.ElementAt(0).First("input").Clear().SendKeys("222");
                cells.ElementAt(1).First("select").Select(2);
                cells.ElementAt(2).First("input").Clear().SendKeys("Ham");
                cells.ElementAt(3).First("input").Clear().SendKeys("1");
                cells.ElementAt(4).First("input").Clear().SendKeys("5");

                // fill fourth line
                cells = table.ElementAt("tr", 4).FindElements("td");
                cells.ElementAt(0).First("input").Clear().SendKeys("333");
                cells.ElementAt(1).First("select").Select(3);
                cells.ElementAt(2).First("input").Clear().SendKeys("Cheese");
                cells.ElementAt(3).First("input").Clear().SendKeys("10");
                cells.ElementAt(4).First("input").Clear().SendKeys("15");

                // verify line totals
                browser.First("input[type=text]").Click();

                AssertUI.InnerTextEquals(table.ElementAt("tr", 2).ElementAt("td", 5), "126");
                AssertUI.InnerTextEquals(table.ElementAt("tr", 3).ElementAt("td", 5), "5.5");
                AssertUI.InnerTextEquals(table.ElementAt("tr", 4).ElementAt("td", 5), "180");

                // recalculate
                recalculateButton.Click();

                // verify total price
                AssertUI.InnerTextEquals(table.Last("tr").ElementAt("th", 1), "407.5");

                // remove second line
                table.ElementAt("tr", 2).Last("td").First("a").Click();

                // verify total price
                AssertUI.InnerTextEquals(table.Last("tr").ElementAt("th", 1), "281.5");
            });
        }

        public InvoiceCalculatorTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
