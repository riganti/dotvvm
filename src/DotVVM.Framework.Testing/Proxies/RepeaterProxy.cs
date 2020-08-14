using System;
using System.Collections.Generic;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.Proxies
{
    public class RepeaterProxy<TItemHelper> : WebElementProxyBase where TItemHelper : SeleniumHelperBase
    {
        public RepeaterProxy(SeleniumHelperBase helper, PathSelector selector) : base(helper, selector)
        {
        }

        public int GetItemsCount()
        {
            return FindElement().FindElements(By.XPath("*")).Count;
        }

        // TODO: CssSelector class
        public TItemHelper GetItem(int index)
        {
            var selector = $"{Helper.BuildElementSelector(Selector)}";

            var sel = new PathSelector
            {
                Index = index,
                Parent = Selector,
                UiName = selector
            };

            return (TItemHelper) Activator.CreateInstance(typeof(TItemHelper), Helper.WebDriver, Helper, sel);
        }

        public IList<TItemHelper> GetItems()
        {
            var returnList = new List<TItemHelper>();
            var children = FindElement().FindElements(By.XPath("./*"));

            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                var selector = new PathSelector
                {
                    Index = i,
                    Parent = Selector,
                    UiName = Helper.BuildElementSelector(Selector)
                };

                var instance = (TItemHelper) Activator.CreateInstance(typeof(TItemHelper), Helper.WebDriver, Helper, selector);
                returnList.Add(instance);
            }

            return returnList;
        }
    }
}
