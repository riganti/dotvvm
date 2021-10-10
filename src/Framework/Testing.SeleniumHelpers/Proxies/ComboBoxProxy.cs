using System;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class ComboBoxProxy : SelectBaseProxy
    {
        public ComboBoxProxy(SeleniumHelperBase helper, PathSelector selector) : base(helper, selector)
        {
        }

        public void SelectPlaceholder()
        {
            SelectOptionByValue("");
        }

        public virtual void SelectOptionByValue(string value)
        {
            var selectElement = GetSelectElement();
            try
            {
                selectElement.SelectByValue(value);
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine($@"ComboBox doesn't have option with value - {value}.");
                throw;
            }
        }
    }
}
