using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers
{
    public abstract class SeleniumHelperBase
    {

        public IWebDriver WebDriver { get; private set; }

        public SeleniumHelperBase(IWebDriver webDriver)
        {
            WebDriver = webDriver;
        }
    }
}
