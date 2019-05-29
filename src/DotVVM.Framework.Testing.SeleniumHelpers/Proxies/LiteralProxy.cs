using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class LiteralProxy : WebElementProxyBase
    {
        public LiteralProxy(SeleniumHelperBase helper, PathSelector selector) : base(helper, selector)
        {
        }

        public string GetText()
        {
            return FindElement().Text;
        }
    }
}
