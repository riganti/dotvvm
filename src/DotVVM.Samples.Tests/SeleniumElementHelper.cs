using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;

namespace DotVVM.Samples.Tests
{
    public class SeleniumElementHelper
    {
        private readonly IWebElement element;

        public SeleniumElementHelper(IWebElement element)
        {
            this.element = element;
        }

        public void Click()
        {
            element.Click();
            Thread.Sleep(100);
        }

        public bool IsDisplayed()
        {
            return element.Displayed;
        }

        public bool IsEnabled()
        {
            return element.Enabled;
        }

        public bool IsSelected()
        {
            return element.Selected;
        }

        public string GetAttribute(string attributeName)
        {
            return element.GetAttribute(attributeName);
        }

        public string GetCssValue(string propertyName)
        {
            return element.GetCssValue(propertyName);
        }

        public string GetText()
        {
            return element.Text;
        }

        public string GetTagName()
        {
            return element.TagName;
        }

        public Point GetLocation()
        {
            return element.Location;
        }

        public Size GetSize()
        {
            return element.Size;
        }

        public void SendKeys(string text)
        {
            element.SendKeys(text);
        }

        public void Clear()
        {
            element.Clear();
        }

        public void Submit()
        {
            element.Submit();
        }

        public SeleniumElementHelper Find(string cssSelector)
        {
            return new SeleniumElementHelper(element.FindElement(By.CssSelector(cssSelector)));
        }

        public List<SeleniumElementHelper> FindAll(string cssSelector)
        {
            return element.FindElements(By.CssSelector(cssSelector)).Select(e => new SeleniumElementHelper(e)).ToList();
        }
    }
}