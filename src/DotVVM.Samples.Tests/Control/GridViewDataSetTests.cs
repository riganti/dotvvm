using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class GridViewDataSetTests : SeleniumTestBase
    {
        [TestMethod]
        public void Control_GridViewDataSet()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("ControlSamples/GridViewDataSet/GridViewDataSet");
                var combobox = browser.First(".GridView");
                browser.Wait();
            });
        }
    }
}
