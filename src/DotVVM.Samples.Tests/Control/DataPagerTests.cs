
using System.Collections.Generic;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class DataPagerTests : AppSeleniumTest
    {
        public DataPagerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_DataPager_DataPager))]
        public void Control_DataPager_DataPager_ShowHideControl()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_DataPager_DataPager);

                // verify the second pager is hidden
                AssertUI.IsDisplayed(browser.First(".pagination"));
                AssertUI.IsNotDisplayed(browser.ElementAt(".pagination", 1));
                browser.First("ul").FindElements("li").ThrowIfDifferentCountThan(2);
                // verify the second pager appears
                browser.Click("input[type=button]");

                // verify the second pager appears
                AssertUI.IsDisplayed(browser.First(".pagination"));
                AssertUI.IsDisplayed(browser.ElementAt(".pagination", 1));
                browser.First("ul").FindElements("li").ThrowIfDifferentCountThan(3);

                // switch to another page
                browser.First(".pagination").ElementAt("li a", 4).Click();

                // verify the second pager is still visible
                AssertUI.IsDisplayed(browser.First(".pagination"));
                AssertUI.IsDisplayed(browser.ElementAt(".pagination", 1));
                browser.First("ul").FindElements("li").ThrowIfDifferentCountThan(3);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_DataPager_DataPager))]
        public void Control_DataPager_DataPager_ActiveCssClass()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_DataPager_DataPager);

                // the first li should be visible because it contains text, the second with the link should be hidden
                var pageIndex1 = browser.First("#pager1").ElementAt("li", 2);
                AssertUI.NotContainsElement(pageIndex1, "a");
                AssertUI.HasClass(pageIndex1, "active");
                AssertUI.IsDisplayed(pageIndex1);

                var pageIndex2 = browser.First("#pager1").ElementAt("li", 3);
                AssertUI.ContainsElement(pageIndex2, "a");
                AssertUI.IsNotDisplayed(pageIndex2);

                // the first li should note be there because only hyperlinks are rendered
                var pageIndex3 = browser.First("#pager3").ElementAt("li", 2);
                AssertUI.ContainsElement(pageIndex3, "a");
                AssertUI.HasClass(pageIndex3, "active");
                AssertUI.IsDisplayed(pageIndex3);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_DataPager_DataPager))]
        public void Control_DataPager_DataPager_DisabledAttribute()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_DataPager_DataPager);

                // the first ul should not be disabled
                AssertUI.HasNotAttribute(browser.Single("#pager1").ElementAt("li a", 0), "disabled");
                AssertUI.HasNotAttribute(browser.Single("#pager1").ElementAt("li a", 1), "disabled");
                AssertUI.HasNotAttribute(browser.Single("#pager1").ElementAt("li a", 2), "disabled");
                AssertUI.HasNotAttribute(browser.Single("#pager1").ElementAt("li a", 3), "disabled");
                AssertUI.HasNotAttribute(browser.Single("#pager1").ElementAt("li a", 4), "disabled");

                // the forth ul should be disabled
                AssertUI.HasAttribute(browser.Single("#pager4").ElementAt("li a", 0), "disabled");
                AssertUI.HasAttribute(browser.Single("#pager4").ElementAt("li a", 1), "disabled");
                AssertUI.HasAttribute(browser.Single("#pager4").ElementAt("li a", 2), "disabled");
                AssertUI.HasAttribute(browser.Single("#pager4").ElementAt("li a", 3), "disabled");
                AssertUI.HasAttribute(browser.Single("#pager4").ElementAt("li a", 4), "disabled");

                // verify element is disabled after click
                browser.Single("#enableCheckbox input[type=checkbox]").Click();
                AssertUI.HasAttribute(browser.Single("#pager1").ElementAt("li a", 0), "disabled");
                AssertUI.HasAttribute(browser.Single("#pager1").ElementAt("li a", 1), "disabled");
                AssertUI.HasAttribute(browser.Single("#pager1").ElementAt("li a", 2), "disabled");
                AssertUI.HasAttribute(browser.Single("#pager1").ElementAt("li a", 3), "disabled");
                AssertUI.HasAttribute(browser.Single("#pager1").ElementAt("li a", 4), "disabled");

                // verify element is not disabled after another click
                browser.Single("#enableCheckbox input[type=checkbox]").Click();
                AssertUI.HasNotAttribute(browser.Single("#pager1").ElementAt("li a", 0), "disabled");
                AssertUI.HasNotAttribute(browser.Single("#pager1").ElementAt("li a", 1), "disabled");
                AssertUI.HasNotAttribute(browser.Single("#pager1").ElementAt("li a", 2), "disabled");
                AssertUI.HasNotAttribute(browser.Single("#pager1").ElementAt("li a", 3), "disabled");
                AssertUI.HasNotAttribute(browser.Single("#pager1").ElementAt("li a", 4), "disabled");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_DataPager_DataPager))]
        public void Control_DataPager_DataPager_DisabledControlClick()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_DataPager_DataPager);

                // populate with data
                browser.Single("populate-button", this.SelectByDataUi).Click();

                // disable pager1 by binding
                var enableCheckbox = browser.Single("#enableCheckbox input[type=checkbox]").ScrollTo().Click();

                // try to switch to next page
                browser.Single("#pager1").ElementAt("li a", browser.Single("#pager1").FindElements("li a").Count - 2).ScrollTo().Click();
                AssertUI.InnerTextEquals(browser.First("ul").First("li"), "Item 0");

                // try to switch to last page
                browser.Single("#pager1").ElementAt("li a", browser.Single("#pager1").FindElements("li a").Count - 1).ScrollTo().Click();
                AssertUI.InnerTextEquals(browser.First("ul").First("li"), "Item 0");

                for (int i = browser.Single("#pager1").FindElements("li a").Count - 3; i > 2; i--)
                {
                    browser.Single("#pager1").ElementAt("li a", i).ScrollTo().Click();
                    AssertUI.InnerTextEquals(browser.First("ul").First("li"), "Item 0");
                }

                // enable pager
                browser.Single("#enableCheckbox input[type=checkbox]").ScrollTo().Click();
                // switch to last page
                browser.Single("#pager1").ElementAt("li a", browser.Single("#pager1").FindElements("li a").Count - 1).ScrollTo().Click();
                // disable pager
                browser.Single("#enableCheckbox input[type=checkbox]").ScrollTo().Click();

                // try to switch to first page
                browser.Single("#pager1").ElementAt("li a", 2).ScrollTo().Click();
                AssertUI.InnerTextEquals(browser.First("ul").First("li"), "Item 48");

                // try to switch to previous page
                browser.Single("#pager1").ElementAt("li a", 1).ScrollTo().Click();
                AssertUI.InnerTextEquals(browser.First("ul").First("li"), "Item 48");

                // try to switch to first
                browser.Single("#pager1").ElementAt("li a", 0).ScrollTo().Click();
                AssertUI.InnerTextEquals(browser.First("ul").First("li"), "Item 48");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_DataPager_DataPager))]
        public void Control_DataPager_DataPager_DisabledByBindingControlClick()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_DataPager_DataPager);

                // populate with data
                browser.Single("populate-button", this.SelectByDataUi).Click();

                // pager 4 should be disabled by value
                // try to switch to next page
                browser.Single("#pager4").ElementAt("li a", browser.Single("#pager4").FindElements("li a").Count - 2).ScrollTo().Click();
                AssertUI.InnerTextEquals(browser.First("ul").First("li"), "Item 0");

                // try to switch to last page
                browser.Single("#pager4").ElementAt("li a", browser.Single("#pager4").FindElements("li a").Count - 1).ScrollTo().Click();
                AssertUI.InnerTextEquals(browser.First("ul").First("li"), "Item 0");

                for (int i = browser.Single("#pager4").FindElements("li a").Count - 3; i > 2; i--)
                {
                    // try to switch to pages
                    browser.Single("#pager4").ElementAt("li a", i).ScrollTo().Click();
                    AssertUI.InnerTextEquals(browser.First("ul").First("li"), "Item 0");
                }
                // switch to last page
                browser.Single("#pager1").ElementAt("li a", browser.Single("#pager1").FindElements("li a").Count - 1).ScrollTo().Click();

                // try to switch to first page
                browser.Single("#pager4").ElementAt("li a", 2).ScrollTo().Click();
                AssertUI.InnerTextEquals(browser.First("ul").First("li"), "Item 48");

                // try to swwitch to previous page
                browser.Single("#pager4").ElementAt("li a", 1).ScrollTo().Click();
                AssertUI.InnerTextEquals(browser.First("ul").First("li"), "Item 48");

                // try to swwitch to first
                browser.Single("#pager4").ElementAt("li a", 0).ScrollTo().Click();
                AssertUI.InnerTextEquals(browser.First("ul").First("li"), "Item 48");
            });
        }

        [Fact]
        public void Control_DataPager_ShowHideControl()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_DataPager_DataPager);

                // verify the second pager is hidden
                AssertUI.IsDisplayed(browser.First(".pagination"));
                AssertUI.IsNotDisplayed(browser.ElementAt(".pagination", 1));
                browser.First("ul").FindElements("li").ThrowIfDifferentCountThan(2);
                // verify the second pager appears
                browser.Single("populate-button", this.SelectByDataUi).Click();

                // verify the second pager appears
                AssertUI.IsDisplayed(browser.First(".pagination"));
                AssertUI.IsDisplayed(browser.ElementAt(".pagination", 1));
                browser.First("ul").FindElements("li").ThrowIfDifferentCountThan(3);

                // switch to another page
                browser.First(".pagination").ElementAt("li a", 4).Click();

                // verify the second pager is still visible
                AssertUI.IsDisplayed(browser.First(".pagination"));
                AssertUI.IsDisplayed(browser.ElementAt(".pagination", 1));
                browser.First("ul").FindElements("li").ThrowIfDifferentCountThan(3);
            });
        }

        [Fact]
        public void Control_DataPager_NearPageIndexes()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_DataPager_DataPager);

                void CheckNearPageIndexes(IEnumerable<int> indexes)
                {
                    var elements = browser.First("#pager1")
                        .FindElements("li:not([style*='none'])");

                    var nearPageIndexesCount = indexes.Count();
                    // Including first page, previous page, next page, last page links
                    elements.ThrowIfDifferentCountThan(nearPageIndexesCount + 4);

                    foreach (var value in indexes.Zip(elements.Skip(2), (i, e) => new { Index = i, Element = e }))
                    {
                        // Skip first and previous links
                        AssertUI.InnerTextEquals(value.Element.Single("span,a"), value.Index.ToString());
                    }
                }

                IElementWrapper GetPageIndex(int index)
                {
                    foreach (var element in browser.Single("#pager1").FindElements("li a"))
                    {
                        if (string.Equals(element.GetInnerText(), index.ToString(), System.StringComparison.InvariantCulture))
                        {
                            return element;
                        }
                    }
                    throw new NoSuchElementException();
                }

                // populate with data
                browser.Single("populate-button", this.SelectByDataUi).Click();

                CheckNearPageIndexes(Enumerable.Range(1, 6));

                GetPageIndex(6).ScrollTo().Click();
                CheckNearPageIndexes(Enumerable.Range(1, 11));

                GetPageIndex(11).ScrollTo().Click();
                CheckNearPageIndexes(Enumerable.Range(6, 11));

                GetPageIndex(16).ScrollTo().Click();
                CheckNearPageIndexes(Enumerable.Range(11, 7));
            });
        }
    }
}
