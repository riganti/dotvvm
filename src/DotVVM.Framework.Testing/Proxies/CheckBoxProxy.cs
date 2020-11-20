namespace DotVVM.Framework.Testing.Proxies
{
    public class CheckBoxProxy : WebElementProxyBase
    {

        public CheckBoxProxy(SeleniumHelperBase helper, PathSelector selector) : base(helper, selector)
        {
        }

        public bool IsChecked()
        {
            return !string.IsNullOrEmpty(FindElement().GetAttribute("checked"));
        }

        public void Check()
        {
            if (!IsChecked())
            {
                FindElement().Click();
            }
        }

        public void Uncheck()
        {
            if (IsChecked())
            {
                FindElement().Click();
            }
        } 
    }
}
