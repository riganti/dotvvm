namespace DotVVM.Framework.Testing.Proxies
{
    public class TextBoxProxy : WebElementProxyBase
    {
        public TextBoxProxy(SeleniumHelperBase helper, PathSelector selector) : base(helper, selector)
        {
        }

        public string GetText()
        {
            // Text property returns empty string
            return FindElement().GetAttribute("value");
        }

        public void SetText(string text)
        {
            var element = FindElement();

            element.Clear();
            element.SendKeys(text);
        }

        public void Clear()
        {
            FindElement().Clear();
        }
    }
}
