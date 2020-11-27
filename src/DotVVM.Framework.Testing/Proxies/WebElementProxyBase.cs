using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.Proxies
{
    public abstract class WebElementProxyBase
    {
        private const string AttributeName = "data-uitest-name";
        private const string AncestorString = "./ancestor::*[not(name()='body' or name()='html') and @data-uitest-name]";

        public SeleniumHelperBase Helper { get; }

        public PathSelector Selector { get; }


        protected WebElementProxyBase(SeleniumHelperBase helper, PathSelector selector)
        {
            Helper = helper;
            Selector = selector;
        }

        public IWebElement GetWebElement()
        {
            return FindElement();
        }

        public IWebElement GetParentElement()
        {
            return FindElement().FindElement(By.XPath("./.."));
        }

        protected IWebElement FindElement()
        {
            var elementsBySelector = GetElementsForCurrentSelector();
            foreach (var element in elementsBySelector)
            {
                IWebElement childElement = element;
                var isElementFound = true;
                var parentSelector = Selector.Parent;
                var childAttribute = Selector.UiName;

                // finds all ancestors of current element which has data-uitest-name attribute
                var ancestors = element.FindElements(By.XPath(AncestorString)).Reverse();
                foreach (var ancestor in ancestors)
                {
                    var ancestorAttribute = ancestor.GetAttribute(AttributeName);

                    if (parentSelector?.Index != null)
                    {
                        // finds all siblings of child of current ancestor
                        var children = GetAllChildren(ancestor, childAttribute);
                        if (children.IndexOf(childElement) != parentSelector.Index)
                        {
                            isElementFound = false;
                            break;
                        }

                        ancestorAttribute = $"//*[@{AttributeName}='{ancestorAttribute}']";
                    }

                    isElementFound = CheckParentConditions(ancestorAttribute, parentSelector);
                    if (!isElementFound)
                    {
                        break;
                    }

                    parentSelector = parentSelector?.Parent;
                    childElement = ancestor;
                    childAttribute = ancestorAttribute;
                }

                if (isElementFound)
                {
                    return element;
                }
            }

            throw new NoSuchElementException();
        }

        private bool CheckParentConditions(string ancestorAttribute, PathSelector parentSelector)
        {
            if (ancestorAttribute != null && parentSelector == null)
            {
                return false;
            }

            if (ancestorAttribute != null && parentSelector.UiName != ancestorAttribute)
            {
                return false;
            }

            return true;
        }

        private IEnumerable<IWebElement> GetElementsForCurrentSelector()
        {
            var elementSelector = Helper.BuildElementSelector(Selector);

            // finds all elements satisfying current element selector
            return Helper.WebDriver.FindElements(By.XPath(elementSelector));
        }

        private ReadOnlyCollection<IWebElement> GetAllChildren(IWebElement ancestor, string childAttribute)
        {
            return ancestor.FindElements(By.XPath($".//*[@{AttributeName}='{childAttribute}']"));
        }

        public virtual bool IsVisible()
        {
            try
            {
                return FindElement().Displayed;
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine(@"Element is not in page. " + e);
                throw;
            }
        }

        public virtual bool IsEnabled()
        {
            try
            {
                return FindElement().Enabled;
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine(@"Element is not in page. " + e);
                throw;
            }
        }
    }
}
