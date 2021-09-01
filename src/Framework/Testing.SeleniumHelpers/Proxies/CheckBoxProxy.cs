namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class CheckBoxProxy : WebElementProxyBase
    {

        public CheckBoxProxy(SeleniumHelperBase helper, string selector) : base(helper, selector)
        {
        }

        public bool IsChecked()
        {
            return !string.IsNullOrEmpty(FindElement().GetAttribute("checked"));
        }

        public void Check(bool isChecked)
        {
            if (IsChecked() != isChecked)
            {
                FindElement().Click();
            }
        }

    }
}