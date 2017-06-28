using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class DataPagerTests : SeleniumTestBase
    {
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

        [TestMethod]
        public void Control_DataPager_DisabledAttribute()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_DataPager_DataPager);
                browser.Wait();

                // the first ul should not be disabled
                browser.Single("#pager1").ElementAt("li a", 0).CheckIfHasNotAttribute("disabled");
                browser.Single("#pager1").ElementAt("li a", 1).CheckIfHasNotAttribute("disabled");
                browser.Single("#pager1").ElementAt("li a", 2).CheckIfHasNotAttribute("disabled");
                browser.Single("#pager1").ElementAt("li a", 3).CheckIfHasNotAttribute("disabled");
                browser.Single("#pager1").ElementAt("li a", 4).CheckIfHasNotAttribute("disabled");

                // the forth ul should be disabled
                browser.Single("#pager4").ElementAt("li a", 0).CheckIfHasAttribute("disabled");
                browser.Single("#pager4").ElementAt("li a", 1).CheckIfHasAttribute("disabled");
                browser.Single("#pager4").ElementAt("li a", 2).CheckIfHasAttribute("disabled");
                browser.Single("#pager4").ElementAt("li a", 3).CheckIfHasAttribute("disabled");
                browser.Single("#pager4").ElementAt("li a", 4).CheckIfHasAttribute("disabled");

                // verify element is disabled after click
                browser.Single("#enableCheckbox input[type=checkbox]").Click();
                browser.Single("#pager1").ElementAt("li a", 0).CheckIfHasAttribute("disabled");
                browser.Single("#pager1").ElementAt("li a", 1).CheckIfHasAttribute("disabled");
                browser.Single("#pager1").ElementAt("li a", 2).CheckIfHasAttribute("disabled");
                browser.Single("#pager1").ElementAt("li a", 3).CheckIfHasAttribute("disabled");
                browser.Single("#pager1").ElementAt("li a", 4).CheckIfHasAttribute("disabled");

                // verify element is not disabled after another click
                browser.Single("#enableCheckbox input[type=checkbox]").Click();
                browser.Single("#pager1").ElementAt("li a", 0).CheckIfHasNotAttribute("disabled");
                browser.Single("#pager1").ElementAt("li a", 1).CheckIfHasNotAttribute("disabled");
                browser.Single("#pager1").ElementAt("li a", 2).CheckIfHasNotAttribute("disabled");
                browser.Single("#pager1").ElementAt("li a", 3).CheckIfHasNotAttribute("disabled");
                browser.Single("#pager1").ElementAt("li a", 4).CheckIfHasNotAttribute("disabled");
            });
        }

        [TestMethod]
        public void Control_DataPager_DisabledByBindingControlClick()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_DataPager_DataPager);
                browser.Wait();

                // populate with data
                browser.Single("populate-button", this.SelectByDataUi).Click();

                // disable pager1 by binding
                var enableCheckbox = browser.Single("#enableCheckbox input[type=checkbox]").ScrollTo().Click();

                // try to switch to next page
                browser.Single("#pager1").ElementAt("li a", browser.Single("#pager1").FindElements("li a").Count - 2).ScrollTo().Click();
                browser.First("ul").First("li").CheckIfInnerTextEquals("Item 0");

                // try to switch to last page
                browser.Single("#pager1").ElementAt("li a", browser.Single("#pager1").FindElements("li a").Count - 1).ScrollTo().Click();
                browser.First("ul").First("li").CheckIfInnerTextEquals("Item 0");

                for (int i = browser.Single("#pager1").FindElements("li a").Count - 3; i > 2; i--)
                {
                    browser.Single("#pager1").ElementAt("li a", i).ScrollTo().Click();
                    browser.First("ul").First("li").CheckIfInnerTextEquals("Item 0");
                }

                // enable pager
                browser.Single("#enableCheckbox input[type=checkbox]").ScrollTo().Click();
                // switch to last page
                browser.Single("#pager1").ElementAt("li a", browser.Single("#pager1").FindElements("li a").Count - 1).ScrollTo().Click();
                // disable pager
                browser.Single("#enableCheckbox input[type=checkbox]").ScrollTo().Click();

                // try to switch to first page
                browser.Single("#pager1").ElementAt("li a", 2).ScrollTo().Click();
                browser.First("ul").First("li").CheckIfInnerTextEquals("Item 18");

                // try to switch to previous page
                browser.Single("#pager1").ElementAt("li a", 1).ScrollTo().Click();
                browser.First("ul").First("li").CheckIfInnerTextEquals("Item 18");

                // try to switch to first
                browser.Single("#pager1").ElementAt("li a", 0).ScrollTo().Click();
                browser.First("ul").First("li").CheckIfInnerTextEquals("Item 18");
            });
        }

        [TestMethod]
        public void Control_DataPager_DisabledControlClick()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_DataPager_DataPager);
                browser.Wait();

                // populate with data
                browser.Single("populate-button", this.SelectByDataUi).Click();

                // pager 4 should be disabled by value
                // try to switch to next page
                browser.Single("#pager4").ElementAt("li a", browser.Single("#pager4").FindElements("li a").Count - 2).ScrollTo().Click().Wait();
                browser.First("ul").First("li").CheckIfInnerTextEquals("Item 0");

                // try to switch to last page
                browser.Single("#pager4").ElementAt("li a", browser.Single("#pager4").FindElements("li a").Count - 1).ScrollTo().Click();
                browser.First("ul").First("li").CheckIfInnerTextEquals("Item 0");

                for (int i = browser.Single("#pager4").FindElements("li a").Count - 3; i > 2; i--)
                {
                    // try to switch to pages
                    browser.Single("#pager4").ElementAt("li a", i).ScrollTo().Click();
                    browser.First("ul").First("li").CheckIfInnerTextEquals("Item 0");
                }
                // switch to last page
                browser.Single("#pager1").ElementAt("li a", browser.Single("#pager1").FindElements("li a").Count - 1).ScrollTo().Click();

                // try to switch to first page
                browser.Single("#pager4").ElementAt("li a", 2).ScrollTo().Click();
                browser.First("ul").First("li").CheckIfInnerTextEquals("Item 18");

                // try to swwitch to previous page
                browser.Single("#pager4").ElementAt("li a", 1).ScrollTo().Click();
                browser.First("ul").First("li").CheckIfInnerTextEquals("Item 18");

                // try to swwitch to first
                browser.Single("#pager4").ElementAt("li a", 0).ScrollTo().Click();
                browser.First("ul").First("li").CheckIfInnerTextEquals("Item 18");
            });
        }

        [TestMethod]
        public void Control_DataPager_ShowHideControl()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_DataPager_DataPager);
                browser.Wait();
                ShowHideControl(browser);
            });
        }

        [TestMethod]
        public void Control_DataPager_ShowHideControlAsync()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_DataPager_DataPager);
                browser.Wait();

                browser.Single("shouldLoadAsync-button", this.SelectByDataUi);

                ShowHideControl(browser);
            });
        }

        private void ShowHideControl(BrowserWrapper browser)
        {
            // verify the second pager is hidden
            browser.First(".pagination").CheckIfIsDisplayed();
            browser.ElementAt(".pagination", 1).CheckIfIsNotDisplayed();
            browser.First("ul").FindElements("li").ThrowIfDifferentCountThan(2);
            // verify the second pager appears
            browser.Single("populate-button", this.SelectByDataUi).Click();

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
        }
    }
}