
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.New.Complex
{
    public class GridViewDataSetTests : AppSeleniumTest
    {
        public GridViewDataSetTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Complex_GridViewDataSet_GridViewDataSet()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_GridViewDataSet_GridViewDataSet);
                browser.First(".GridView");

                var buttonsInGridView = browser.FindElements(SelectByDataUiId("button-with-html-content"));

                foreach (var button in buttonsInGridView)
                {
                    AssertUI.ContainsElement(button, "h4");
                    AssertUI.InnerTextEquals(button, "Choose");
                }
            });
        }
        
        protected By SelectByDataUiId(string selector)
          => By.CssSelector($"[data-ui='{selector}']");

    }
}
