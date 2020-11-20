using System;
using System.Collections.Generic;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.Proxies
{
    public class GridViewProxy<TItemHelper> : WebElementProxyBase where TItemHelper : SeleniumHelperBase
    {
        public GridViewProxy(SeleniumHelperBase helper, PathSelector selector) : base(helper, selector)
        {
        }

        public IList<IWebElement> GetVisibleRows()
        {
            var tableBody = GetTableBody();

            return tableBody.FindElements(By.TagName("tr"));
        }

        public IWebElement GetTableBody()
        {
            var element = FindElement();

            return element.FindElement(By.TagName("tbody"));
        }

        public IWebElement GetHeader()
        {
            var element = FindElement();

            return element.FindElement(By.TagName("thead"));
        }

        public int GetVisibleRowsCount()
        {
            return GetVisibleRows().Count;
        }

        public int GetColumnsCount()
        {
            var header = GetHeader();

            var columnsHeaders = header.FindElements(By.XPath("//tr/th"));

            return columnsHeaders.Count;
        }

        public TItemHelper GetItem(int index)
        {
            //var selector = $"{Helper.BuildElementSelector(Selector)} > tbody >*:nth-child({index + 1})";

            var uiName = $"{Helper.BuildElementSelector(Selector)}";

            var selector = new PathSelector
            {
                Index = index,
                Parent = Selector,
                UiName = uiName
            };

            return (TItemHelper)Activator.CreateInstance(typeof(TItemHelper), Helper.WebDriver, Helper, selector);
        }

        //public IList<IWebElement> GetTableCellsByValue(string value)
        //{
        //    var tableBody = GetTableBody();

        //    var expression = $"//table[@data-uitest-name='{Selector}']//text()[.='{value}']/..";
        //    var elementsByValue = tableBody.FindElements(By.XPath(expression));

        //    var returnElements = new List<IWebElement>();
        //    foreach (var element in elementsByValue)
        //    {
        //        var parentElement = GetTableCell(element);
        //        if (parentElement != null)
        //        {
        //            returnElements.Add(parentElement);
        //        }
        //    }

        //    return returnElements;
        //}

        //private IWebElement GetTableCell(IWebElement element)
        //{
        //    var parent = element.FindElement(By.XPath(".."));
        //    if (parent.TagName == "td")
        //    {
        //        return parent;
        //    }

        //    if(parent.TagName == "tbody" || parent.TagName == "body")
        //    {
        //        return null;
        //    }

        //    return GetTableCell(parent);
        //}

        //public IWebElement GetTableCellByDimensions(int column, int row)
        //{
        //    var tableBody = GetTableBody();

        //    var selector = $"tr:nth-child({row}) > td:nth-child({column})";

        //    return tableBody.FindElement(By.CssSelector(selector));
        //}
    }
}
