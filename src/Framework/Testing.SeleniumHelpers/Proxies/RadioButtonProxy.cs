using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class RadioButtonProxy : WebElementProxyBase
    {
        public RadioButtonProxy(SeleniumHelperBase helper, PathSelector selector) : base(helper, selector)
        {
        }

        public bool IsSelected()
        {
            var element = FindElement();
            var inputElement = element.FindElement(By.TagName("input"));

            return inputElement.Selected;
        }

        public void Select()
        {
            FindElement().Click();
        }
    }
}
