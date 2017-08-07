using OpenQA.Selenium;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests
{
    public static class SeleniumTestBaseExtensions
    {
        public static By SelectByDataUi(this SeleniumTestBase testBase, string selector)
        => By.CssSelector($"[data-ui='{selector}']");
    }
}