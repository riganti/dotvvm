using OpenQA.Selenium;
using Riganti.Selenium.Core;

namespace DotVVM.Samples.Tests
{
    public static class SeleniumTestBaseExtensions
    {
        public static By SelectByDataUi(this ISeleniumTest testBase, string selector)
        => By.CssSelector($"[data-ui='{selector}']");
    }
}