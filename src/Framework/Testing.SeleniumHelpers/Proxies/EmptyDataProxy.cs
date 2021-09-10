using System;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class EmptyDataProxy : WebElementProxyBase
    {
        public EmptyDataProxy(SeleniumHelperBase helper, PathSelector selector) : base(helper, selector)
        {
        }

        public override bool IsVisible()
        {
            try
            {
                return FindElement().Displayed;
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine(@"EmptyData is not visible" + e);
                throw;
            }
        }
    }
}
