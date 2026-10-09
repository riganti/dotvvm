using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Selenium.Core;
using Xunit;

namespace DotVVM.Samples.Tests.Feature
{
    public class KeyboardShortcutTests : AppSeleniumTest
    {
        public KeyboardShortcutTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_KeyboardShortcuts_KeyboardShortcuts))]
        public void Feature_KeyboardShortcuts_DocumentAndEnabled()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_KeyboardShortcuts_KeyboardShortcuts);

                SendShortcut(browser, Keys.Control, "s");
                AssertUI.InnerTextEquals(browser.Single("document-count", SelectByDataUi), "1");

                browser.Single("document-enabled", SelectByDataUi).Click();
                SendShortcut(browser, Keys.Control, "s");
                AssertUI.InnerTextEquals(browser.Single("document-count", SelectByDataUi), "1");

                SendShortcut(browser, Keys.Alt, "s");
                AssertUI.InnerTextEquals(browser.Single("document-count", SelectByDataUi), "1");
            });
        }

        [Fact]
        public void Feature_KeyboardShortcuts_TextAreaCtrlEnter()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_KeyboardShortcuts_KeyboardShortcuts);
                var draft = browser.Single("draft", SelectByDataUi);
                draft.SendKeys("first line").SendKeys(Keys.Enter).SendKeys("second line");

                AssertUI.InnerTextEquals(browser.Single("submission-count", SelectByDataUi), "0");
                new Actions(browser.Driver)
                    .KeyDown(Keys.Control)
                    .SendKeys(Keys.Enter)
                    .KeyUp(Keys.Control)
                    .Perform();

                AssertUI.InnerTextEquals(browser.Single("submission-count", SelectByDataUi), "1");
                AssertUI.InnerTextEquals(browser.Single("submitted-text", SelectByDataUi), "first line second line");
            });
        }

        [Fact]
        public void Feature_KeyboardShortcuts_NestedModalScope()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_KeyboardShortcuts_KeyboardShortcuts);
                browser.Single("open-outer", SelectByDataUi).Click();
                var outer = browser.Single("outer-dialog", SelectByDataUi);
                AssertUI.IsDisplayed(outer);

                outer.SendKeys(Keys.Escape);
                AssertUI.InnerTextEquals(browser.Single("outer-count", SelectByDataUi), "1");
                AssertUI.InnerTextEquals(browser.Single("document-count", SelectByDataUi), "0");
                AssertUI.IsDisplayed(outer);

                browser.Single("open-inner", SelectByDataUi).Click();
                var inner = browser.Single("inner-dialog", SelectByDataUi);
                AssertUI.IsDisplayed(inner);
                inner.SendKeys(Keys.Escape);

                AssertUI.InnerTextEquals(browser.Single("inner-count", SelectByDataUi), "1");
                AssertUI.InnerTextEquals(browser.Single("outer-count", SelectByDataUi), "1");
                AssertUI.InnerTextEquals(browser.Single("document-count", SelectByDataUi), "0");
                AssertUI.IsDisplayed(inner);
            });
        }

        private static void SendShortcut(Riganti.Selenium.Core.Abstractions.IBrowserWrapper browser, string modifier, string key)
        {
            new Actions(browser.Driver)
                .KeyDown(modifier)
                .SendKeys(key)
                .KeyUp(modifier)
                .Perform();
        }
    }
}
