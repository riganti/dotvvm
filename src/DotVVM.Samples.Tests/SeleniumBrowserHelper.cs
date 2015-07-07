using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;

namespace DotVVM.Samples.Tests
{
    public class SeleniumBrowserHelper : IDisposable
    {
        private readonly IWebDriver browser;

        public SeleniumBrowserHelper(IWebDriver browser)
        {
            this.browser = browser;
        }

        public string CurrentUrl
        {
            get { return browser.Url; }
        }


        public void Click(string cssSelector)
        {
            browser.FindElement(By.CssSelector(cssSelector)).Click();
            Thread.Sleep(100);
        }

        public bool IsDisplayed(string cssSelector)
        {
            return browser.FindElement(By.CssSelector(cssSelector)).Displayed;
        }

        public bool IsEnabled(string cssSelector)
        {
            return browser.FindElement(By.CssSelector(cssSelector)).Enabled;
        }

        public bool IsSelected(string cssSelector)
        {
            return browser.FindElement(By.CssSelector(cssSelector)).Selected;
        }

        public string GetAttribute(string cssSelector, string attributeName)
        {
            return browser.FindElement(By.CssSelector(cssSelector)).GetAttribute(attributeName);
        }

        public string GetCssValue(string cssSelector, string propertyName)
        {
            return browser.FindElement(By.CssSelector(cssSelector)).GetCssValue(propertyName);
        }

        public string GetText(string cssSelector)
        {
            return browser.FindElement(By.CssSelector(cssSelector)).Text;
        }

        public string GetTagName(string cssSelector)
        {
            return browser.FindElement(By.CssSelector(cssSelector)).TagName;
        }

        public Point GetLocation(string cssSelector)
        {
            return browser.FindElement(By.CssSelector(cssSelector)).Location;
        }

        public Size GetSize(string cssSelector)
        {
            return browser.FindElement(By.CssSelector(cssSelector)).Size;
        }

        public void SendKeys(string cssSelector, string text)
        {
            browser.FindElement(By.CssSelector(cssSelector)).SendKeys(text);
        }

        public void Clear(string cssSelector)
        {
            browser.FindElement(By.CssSelector(cssSelector)).Clear();
        }

        public void Submit(string cssSelector)
        {
            browser.FindElement(By.CssSelector(cssSelector)).Submit();
        }

        public SeleniumElementHelper Find(string cssSelector)
        {
            return new SeleniumElementHelper(browser.FindElement(By.CssSelector(cssSelector)));
        }

        public List<SeleniumElementHelper> FindAll(string cssSelector)
        {
            return browser.FindElements(By.CssSelector(cssSelector)).Select(e => new SeleniumElementHelper(e)).ToList();
        }


        public string GetAlertText()
        {
            var alert = browser.SwitchTo().Alert();
            if (alert != null)
            {
                return alert.Text;
            }
            return null;
        }

        public void ConfirmAlert()
        {
            browser.SwitchTo().Alert().Accept();
            Thread.Sleep(500);
        }

        public void NavigateToUrl(string url)
        {
            browser.Navigate().GoToUrl(url);
        }

        public void NavigateBack()
        {
            browser.Navigate().Back();
        }

        public void NavigateForward()
        {
            browser.Navigate().Forward();
        }

        public void Refresh()
        {
            browser.Navigate().Refresh();
        }

        /// <summary>
        /// Takes a screenshot and returns a full path to the file.
        /// </summary>
        public void TakeScreenshot(string filename)
        {
            ((ITakesScreenshot)browser).GetScreenshot().SaveAsFile(filename, ImageFormat.Png);
        }

        public void Dispose()
        {
            browser.Dispose();
        }
    }
}