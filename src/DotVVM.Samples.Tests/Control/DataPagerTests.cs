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
        public void Control_DataPager_ShowHideControl()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_DataPager_DataPager);
                browser.Wait();

                // verify the second pager is hidden
                browser.First(".pagination").CheckIfIsDisplayed();
                browser.ElementAt(".pagination", 1).CheckIfIsNotDisplayed();
                browser.First("ul").FindElements("li").ThrowIfDifferentCountThan(2);
                // verify the second pager appears
                browser.Click("input[type=button]");

                // verify the second pager appears
                browser.First(".pagination").CheckIfIsDisplayed();
                browser.ElementAt(".pagination", 1).CheckIfIsDisplayed();
                browser.First("ul").FindElements("li").ThrowIfDifferentCountThan(3);

                // switch to another page
                browser.First(".pagination").ElementAt("li a", 4).Click();

                // verify the second pager is still visible
                browser.First(".pagination").CheckIfIsDisplayed();
                browser.ElementAt(".pagination", 1).CheckIfIsDisplayed();
                browser.First("ul").FindElements("li").ThrowIfDifferentCountThan(3);
            });
        }

        [TestMethod]
        public void Control_DataPager_ActiveCssClass()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_DataPager_DataPager);
                browser.Wait();

                // the first li should be visible because it contains text, the second with the link should be hidden
                browser.ElementAt(".pagination", 0).ElementAt("li", 2).CheckIfNotContainsElement("a").CheckIfHasClass("active").CheckIfIsDisplayed();
                browser.ElementAt(".pagination", 0).ElementAt("li", 3).CheckIfContainsElement("a").CheckIfHasClass("active").CheckIfIsNotDisplayed();

                // the first li should note be there because only hyperlinks are rendered
                browser.ElementAt(".pagination", 2).ElementAt("li", 2).CheckIfContainsElement("a").CheckIfHasClass("active").CheckIfIsDisplayed();
            });
        }
    }
}
