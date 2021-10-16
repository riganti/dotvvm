using OpenQA.Selenium.Interactions;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class ButtonProxy : WebElementProxyBase, IButtonProxyBase
    {
        public ButtonProxy(SeleniumHelperBase helper, PathSelector selector) : base(helper, selector)
        {
        }

        public void Click()
        {
            FindElement().Click();
        }

        public void DoubleClick()
        {
            var element = FindElement();
            new Actions(Helper.WebDriver).DoubleClick(element).Perform();
        }

        public void ClickAndHold()
        {
            var element = FindElement();
            new Actions(Helper.WebDriver).ClickAndHold(element).Perform();
        }
    }
}
