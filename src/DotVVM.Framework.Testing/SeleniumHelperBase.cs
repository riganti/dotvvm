using OpenQA.Selenium;

namespace DotVVM.Framework.Testing
{
    public abstract class SeleniumHelperBase
    {
        private const string AttributeName = "data-uitest-name";

        public IWebDriver WebDriver { get; }

        public SeleniumHelperBase ParentHelper { get; set; }

        public PathSelector ParentSelector { get; }


        protected SeleniumHelperBase(IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null)
        {
            WebDriver = webDriver;
            ParentHelper = parentHelper;
            ParentSelector = parentSelector;
        }

        public string BuildElementSelector(PathSelector elementUniqueName)
        {
            var xpathSelector = $"//*[@{AttributeName}='{elementUniqueName.UiName}']";

            if (ParentSelector == null)
            {
                return xpathSelector;
            }

            if (ParentSelector?.Index != null)
            {
                return $"{ParentSelector.ToString()}{elementUniqueName}";
            }

            return $"{ParentSelector.ToString()}{xpathSelector}";
        }
        
    }
}
