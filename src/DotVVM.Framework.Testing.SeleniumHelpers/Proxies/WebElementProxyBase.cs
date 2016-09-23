using System.Linq;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public abstract class WebElementProxyBase
    {
        public SeleniumHelperBase Helper { get; private set; }

        public string Selector { get; private set; }


        public WebElementProxyBase(SeleniumHelperBase helper, string selector)
        {
            Helper = helper;
            Selector = selector;
        }

        protected IWebElement FindElement()
        {
            return Helper.WebDriver.FindElement(UITestSelector(Selector));
        }

        private By UITestSelector(string selector)
        {
            var css = string.Join(" ", selector.Split(' ').Select(p => $"[data-uitest-name={p}]"));
            return By.CssSelector(css);
        }
    }
    
}