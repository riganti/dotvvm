using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class DataPagerProxy : WebElementProxyBase
    {
        public DataPagerProxy(SeleniumHelperBase helper, PathSelector selector) : base(helper, selector)
        {
        }

        public override bool IsVisible()
        {
            return FindElement().Displayed;
        }

        private IEnumerable<IWebElement> GetDataPagerListItems()
        {
            var listElement = FindElement();
            return listElement.FindElements(By.TagName("li")).Where(e => e.Displayed);
        }

        public void GoToFirstPage()
        {
            var firstItem = GetDataPagerListItems().FirstOrDefault();
            if (firstItem != null && firstItem.Enabled)
            {
                var anchor = GetAnchor(firstItem);
                anchor.Click();
            }
        }

        private static IWebElement GetAnchor(IWebElement firstItem)
        {
            var anchor = firstItem.FindElement(By.TagName("a"));
            return anchor;
        }

        public void GoToLastPage()
        {
            var items = GetDataPagerListItems().ToList();
            var lastItem = items.LastOrDefault();
            if (lastItem != null && lastItem.Enabled)
            {
                var anchor = GetAnchor(lastItem);
                anchor.Click();
            }
        }

        public void GoToPreviousPage()
        {
            var secondItem = GetDataPagerListItems().Skip(1).FirstOrDefault();
            if (secondItem != null && secondItem.Enabled)
            {
                var anchor = GetAnchor(secondItem);
                anchor.Click();
            }
        }

        public void GoToNextPage()
        {
            var items = GetDataPagerListItems().ToList();

            if(items.Count > 1)
            {
                var secondLastItem = items[items.Count - 2];
                if (secondLastItem != null && secondLastItem.Enabled)
                {
                    var anchor = GetAnchor(secondLastItem);
                    anchor.Click();
                }
            }
        }

        public void GoToPage(int pageNumber)
        {
            var items = GetDataPagerListItems().ToList();

            if (items.Count > 4)
            {
                // skip first previous buttons, next and last
                items = SkipArrowsButtons(items);

                if (pageNumber <= items.Count)
                {
                    var anchor = GetAnchor(items[pageNumber - 1]);
                    anchor.Click();
                }
            }
            else
            {
                Console.WriteLine($@"DataPager doesn't have {pageNumber}. page.");
            }
        }

        public int GetPageCount()
        {
            var items = GetDataPagerListItems().ToList();
            items = SkipArrowsButtons(items);

            return items.Count;
        }

        private List<IWebElement> SkipArrowsButtons(List<IWebElement> items)
        {
            items = items.Skip(2).ToList();
            return items.GetRange(0, items.Count - 2);
        }
    }
}
