using System.Collections.Generic;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.Proxies.Interfaces
{
    public interface ISelectProxy
    {
        bool SelectOptionByContent(string content);
        bool SelectOptionByIndex(int optionIndex);
        IWebElement GetSelectedOption();
        IEnumerable<IWebElement> GetOptions();
        IEnumerable<IWebElement> GetOptions(IWebElement element);
    }
}
