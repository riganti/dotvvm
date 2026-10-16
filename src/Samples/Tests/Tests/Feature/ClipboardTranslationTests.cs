using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class ClipboardTranslationTests : AppSeleniumTest
    {
        private const string SampleRoute = "FeatureSamples/JavascriptTranslation/ClipboardTranslation";

        [Fact]
        [SampleReference(SampleRoute)]
        public void Feature_JavascriptTranslation_Clipboard()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SampleRoute);
                var textToCopy = "Selenium clipboard test";
                browser.First("text-to-copy", SelectByDataUi).Clear().SendKeys(textToCopy);
                browser.First("copy", SelectByDataUi).Click();

                AssertUI.TextEquals(browser.First("clipboard-status", SelectByDataUi), "Copied");
                var pasteTarget = browser.First("paste-target", SelectByDataUi);
                pasteTarget.Click();
                pasteTarget.SendKeys(Keys.Control + "v");
                Assert.Equal(textToCopy, pasteTarget.GetAttribute("value"));
            });
        }

        public ClipboardTranslationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
