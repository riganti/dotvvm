using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ViewModelNestingTests : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_ViewModelNesting_NestedViewModel()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ViewModelNesting_NestedViewModel);

                // check table values
                CheckTableRow(browser, 1);
                CheckTableRow(browser, 2);
                CheckTableRow(browser, 3);

                // check tree structure
                CheckTreeItems(browser, 0, 5); // 5 nodes in level 0
                CheckTreeItems(browser, 1, 20); // 20 nodes in level 1
                CheckTreeItems(browser, 2, 60); // 60 nodes in level 2
                CheckTreeItems(browser, 3, 120); // 120 nodes in level 3
                CheckTreeItems(browser, 4, 120); // 120 nodes in level 4
            });
        }

        private void CheckTreeItems(IBrowserWrapperFluentApi browser, int level, int count)
        {
            browser.FindElements($"[data-ui='offset_{level}']").ThrowIfDifferentCountThan(count);
        }

        private void CheckTableRow(IBrowserWrapperFluentApi browser, int row)
        {
            var table = browser.Single("table");

            // get expected value - last column
            var value = table.ElementAt("tr", row).ElementAt("td", 3).GetInnerText();

            // check other columns to contain same value
            table.ElementAt("tr", row).ElementAt("td", 1).CheckIfInnerTextEquals(value, false);
            table.ElementAt("tr", row).ElementAt("td", 2).CheckIfInnerTextEquals(value, false);

            // server binding renders True with capital T, knockout binding renders true with lower case t -> comparison is case insensitive
        }
    }
}