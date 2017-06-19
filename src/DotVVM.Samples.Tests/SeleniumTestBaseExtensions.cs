using OpenQA.Selenium;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests
{
    public static class SeleniumTestBaseExtensions
    {
        public static By SelectByDataUiId(this SeleniumTestBase testBase, string selector)
        => By.CssSelector($"[data-ui='{selector}']");
    }
}