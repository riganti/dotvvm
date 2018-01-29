
using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using Riganti.Selenium.Core;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class GridViewDataSetTests : AppSeleniumTest
    {
        [TestMethod]
        public void Complex_GridViewDataSet_GridViewDataSet()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_GridViewDataSet_GridViewDataSet);
                browser.First(".GridView");
            });
        }
        
        protected By SelectByDataUiId(string selector)
          => By.CssSelector($"[data-ui='{selector}']");

    }
}
