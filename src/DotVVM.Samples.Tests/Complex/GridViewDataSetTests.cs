using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class GridViewDataSetTests : SeleniumTestBase
    {
        [TestMethod]
        public void Complex_GridViewDataSet()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_GridViewDataSet_GridViewDataSet);
                browser.First(".GridView");
            });
        }
    }
}
