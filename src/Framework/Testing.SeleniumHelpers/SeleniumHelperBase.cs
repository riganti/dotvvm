using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers
{
    public abstract class SeleniumHelperBase
    {

        public IWebDriver WebDriver { get; private set; }

        public SeleniumHelperBase ParentHelper { get; set; }

        public string SelectorPrefix { get; private set; }


        public SeleniumHelperBase(IWebDriver webDriver, SeleniumHelperBase parentHelper = null, string selectorPrefix = "")
        {
            WebDriver = webDriver;
            ParentHelper = parentHelper;
            SelectorPrefix = selectorPrefix;
        }

        public string BuildElementSelector(string elementUniqueName)
        {
            var selector = $"[data-uitest-name={elementUniqueName}]";

            if (string.IsNullOrEmpty(SelectorPrefix))
            {
                return selector;
            }
            else
            {
                return SelectorPrefix + " " + selector;
            }
        }
        
    }
}
