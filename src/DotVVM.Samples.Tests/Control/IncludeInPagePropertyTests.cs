using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class IncludeInPagePropertyTests : SeleniumTest
    {
        [TestMethod]
        public void Control_IncludeInPageProperty_IncludeInPage()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_IncludeInPageProperty_IncludeInPage);

                var textBox = browser.Single("textbox", this.SelectByDataUi);
                textBox.CheckIfIsDisplayed();

                var repeaterLiterals = browser.FindElements("repeater-literal", this.SelectByDataUi);
                foreach (var literal in repeaterLiterals)
                {
                    literal.CheckIfIsDisplayed();
                }

                var singleLiteral = browser.Single("literal", this.SelectByDataUi);
                singleLiteral.CheckIfIsDisplayed();

                browser.Single("switch", this.SelectByDataUi).Click();

                Assert.AreEqual(0, browser.FindElements("textbox", this.SelectByDataUi).Count);
                Assert.AreEqual(0, browser.FindElements("repeater-literal", this.SelectByDataUi).Count);
                Assert.AreEqual(0, browser.FindElements("literal", this.SelectByDataUi).Count);
            });
        }
    }
}