using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;

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
            browser.SingleByCssSelector(cssSelector).Click();
            Thread.Sleep(100);
        }

        public bool IsDisplayed(string cssSelector)
        {
            return browser.SingleByCssSelector(cssSelector).Displayed;
        }

        public bool IsEnabled(string cssSelector)
        {
            return browser.SingleByCssSelector(cssSelector).Enabled;
        }

        public bool IsSelected(string cssSelector)
        {
            return browser.SingleByCssSelector(cssSelector).Selected;
        }

        public string GetAttribute(string cssSelector, string attributeName)
        {
            return browser.SingleByCssSelector(cssSelector).GetAttribute(attributeName);
        }

        public string GetCssValue(string cssSelector, string propertyName)
        {
            return browser.SingleByCssSelector(cssSelector).GetCssValue(propertyName);
        }

        public string GetText(string cssSelector)
        {
            return browser.SingleByCssSelector(cssSelector).Text;
        }

        public string GetTagName(string cssSelector)
        {
            return browser.SingleByCssSelector(cssSelector).TagName;
        }

        public Point GetLocation(string cssSelector)
        {
            return browser.SingleByCssSelector(cssSelector).Location;
        }

        public Size GetSize(string cssSelector)
        {
            return browser.SingleByCssSelector(cssSelector).Size;
        }

        public void SendKeys(string cssSelector, string text)
        {
            browser.SingleByCssSelector(cssSelector).SendKeys(text);
        }

        public void Clear(string cssSelector)
        {
            browser.SingleByCssSelector(cssSelector).Clear();
        }

        public void Submit(string cssSelector)
        {
            browser.SingleByCssSelector(cssSelector).Submit();
        }

        public SeleniumElementHelper Find(string cssSelector)
        {
            return new SeleniumElementHelper(browser.SingleByCssSelector(cssSelector));
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
            Thread.Sleep(500);
        }

        public void NavigateBack()
        {
            browser.Navigate().Back();
            Thread.Sleep(500);
        }

        public void NavigateForward()
        {
            browser.Navigate().Forward();
            Thread.Sleep(500);
        }

        public void Refresh()
        {
            browser.Navigate().Refresh();
            Thread.Sleep(500);
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