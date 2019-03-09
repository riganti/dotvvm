using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class ViewModelNestingTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_ViewModelNesting_NestedViewModel()
        {
            RunInAllBrowsers(browser => {
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

        private void CheckTreeItems(IBrowserWrapper browser, int level, int count)
        {
            browser.FindElements($"[data-ui='offset_{level}']").ThrowIfDifferentCountThan(count);
        }

        private void CheckTableRow(IBrowserWrapper browser, int row)
        {
            var table = browser.Single("table");

            // get expected value - last column
            var value = table.ElementAt("tr", row).ElementAt("td", 3).GetInnerText();

            // check other columns to contain same value
            AssertUI.InnerTextEquals(table.ElementAt("tr", row).ElementAt("td", 1), value);
            AssertUI.InnerTextEquals(table.ElementAt("tr", row).ElementAt("td", 2), value, false);

            // server binding renders True with capital T, knockout binding renders true with lower case t -> comparison is case insensitive
        }

        public ViewModelNestingTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
