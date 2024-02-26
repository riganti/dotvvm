using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class CompilationPageTest : AppSeleniumTest
    {
        IElementWrapper TableRow(IBrowserWrapper browser, string name) =>
            browser.Single($"//tr[not(contains(@class, 'row-continues')) and td[normalize-space(.) = '{name}']]", By.XPath);
        [Fact]
        [Trait("Category", "dev-only")]
        public void Feature_CompilationPage_SmokeTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl("/_dotvvm/diagnostics/compilation");
                browser.WaitFor(() => { browser.Single("compile-all-button", By.Id).Click(); }, timeout: 15_000);
                browser.Single("Routes", SelectByButtonText).Click();

                // shows failed pages
                browser.WaitFor(() => {
                    Assert.InRange(browser.FindElements("tbody tr.success").Count, 10, int.MaxValue);
                    Assert.InRange(browser.FindElements("tbody tr.failure").Count, 10, int.MaxValue);
                    var failedRow = () => TableRow(browser, "FeatureSamples_CompilationPage_BindingsTestError");
                    AssertUI.InnerTextEquals(failedRow().ElementAt("td", 1), "FeatureSamples/CompilationPage/BindingsTestError");
                    AssertUI.InnerTextEquals(failedRow().ElementAt("td", 3), "CompilationFailed");
                    AssertUI.HasClass(failedRow(), "failure", waitForOptions: WaitForOptions.Disabled);
                }, timeout: 60_000);
                AssertUI.HasNotClass(TableRow(browser, "FeatureSamples_CompilationPage_BindingsTest"), "failure");

                // shows some errors and warnings
                Assert.InRange(browser.FindElements(".source-errorLine").Count, 20, int.MaxValue);
                Assert.InRange(browser.FindElements(".source-warningLine").Count, 20, int.MaxValue);

                // found master pages
                browser.Single("Master pages", SelectByButtonText).Click();
                AssertUI.IsDisplayed(TableRow(browser, "Views/Errors/Master.dotmaster"));
                AssertUI.IsDisplayed(TableRow(browser, "Views/ControlSamples/SpaContentPlaceHolder_HistoryApi/SpaMaster.dotmaster"));

                // found controls
                browser.Single("Controls", SelectByButtonText).Click();
                AssertUI.IsDisplayed(TableRow(browser, "MarkupControlPropertiesSameName"));

                // filters errors and warnings
                browser.Single("Errors", SelectByButtonText).Click();
                browser.FindElements("tbody tr:not(.failure):not(.row-continues)").ThrowIfDifferentCountThan(0);

                browser.Single("Warnings", SelectByButtonText).Click();
                browser.FindElements("tbody tr.success").ThrowIfDifferentCountThan(0);
            });
        }

        public CompilationPageTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}
