using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class GridViewDataSetTests : SeleniumTest
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

        [TestMethod]
        public void Complex_GridViewDataSet_GridViewDataSetDelegate()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_GridViewDataSet_GridViewDataSetDelegate);

                var counter = browser.First("CallDelegateCounter", SelectByDataUiId);
                //init load
                counter.CheckIfInnerText(text => text == "1");


                var datapager1 = browser.First("DataPager1", SelectByDataUiId);
                datapager1.ElementAt("li a", 4).Click();

                //second reload
                counter.CheckIfInnerText(text => text == "2");

                var datapager2 = browser.First("DataPager2", SelectByDataUiId);
                datapager2.ElementAt("li a", 5).Click();

                //third reload
                counter.CheckIfInnerText(text => text == "3");

                //fourth reload
                datapager1.ElementAt("li a", 1).Click();
                counter.CheckIfInnerText(text => text == "4");

            });
        }
        protected By SelectByDataUiId(string selector)
          => By.CssSelector($"[data-ui='{selector}']");

    }
}
