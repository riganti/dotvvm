using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class DataPagerTests : SeleniumTestBase
    {
        [TestMethod]
        public void Control_DataPager()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_DataPager);
                browser.Wait();

                // verify the second pager is hidden
                browser.First(".pagination").CheckIfIsDisplayed();
                browser.Last(".pagination").CheckIfIsNotDisplayed();
                browser.First("ul").FindElements("li").ThrowIfDifferentCountThan(2);
                // verify the second pager appears
                browser.Click("input[type=button]");
                browser.Wait();

                // verify the second pager appears
                browser.First(".pagination").CheckIfIsDisplayed();
                browser.Last(".pagination").CheckIfIsDisplayed();
                browser.First("ul").FindElements("li").ThrowIfDifferentCountThan(3);

                // switch to another page
                browser.First(".pagination").ElementAt("li a", 4).Click();
                browser.Wait();

                // verify the second pager is still visible
                browser.First(".pagination").CheckIfIsDisplayed();
                browser.Last(".pagination").CheckIfIsDisplayed();
                browser.First("ul").FindElements("li").ThrowIfDifferentCountThan(3);
            });
        }
    }
}
