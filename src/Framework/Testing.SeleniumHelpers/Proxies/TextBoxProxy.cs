using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class TextBoxProxy : WebElementProxyBase
    {
        public TextBoxProxy(SeleniumHelperBase helper, string selector) : base(helper, selector)
        {
        }

        public string GetText()
        {
            return FindElement().Text;
        }

        public void SetText(string text)
        {
            FindElement().SendKeys(text);
        }

        public void Clear()
        {
            FindElement().Clear();
        }
    }
}
