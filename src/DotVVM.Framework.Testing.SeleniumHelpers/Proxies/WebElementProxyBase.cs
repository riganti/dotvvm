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
            var selector = Helper.BuildElementSelector(Selector);
            return Helper.WebDriver.FindElement(By.CssSelector(selector));
        }

    }
    
}