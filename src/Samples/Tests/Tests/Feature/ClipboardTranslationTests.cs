using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
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
                browser.First("text-to-copy", SelectByDataUi).Clear().SendKeys("Selenium clipboard test");
                browser.First("copy", SelectByDataUi).Click();

                AssertUI.TextEquals(browser.First("clipboard-status", SelectByDataUi), "Copied");
            });
        }

        public ClipboardTranslationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
