using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class SelectBaseProxy : WebElementProxyBase, ISelectProxy
    {
        public SelectBaseProxy(SeleniumHelperBase helper, PathSelector selector) : base(helper, selector)
        {
        }

        public virtual bool SelectOptionByContent(string content)
        {
            var selectElement = GetSelectElement();

            try
            {
                selectElement.SelectByText(content);
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine($@"ComboBox doesn't have option with text - {content}.");
                return false;
            }

            return true;
        }

        public virtual bool SelectOptionByIndex(int optionIndex)
        {
            var selectElement = GetSelectElement();

            try
            {
                selectElement.SelectByIndex(optionIndex);
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine($@"ComboBox doesn't have option with index {optionIndex}.");
                return false;
            }

            return true;
        }

        public virtual IWebElement GetSelectedOption()
        {
            var selectElement = GetSelectElement();

            return selectElement.SelectedOption;
        }

        protected SelectElement GetSelectElement()
        {
            var element = FindElement();
            return new SelectElement(element);
        }

        public virtual IEnumerable<IWebElement> GetOptions(IWebElement element)
            => element.FindElements(By.XPath(".//*"));

        public virtual IEnumerable<IWebElement> GetOptions()
        {
            var element = FindElement();
            return element.FindElements(By.XPath(".//*"));
        }
    }
}
