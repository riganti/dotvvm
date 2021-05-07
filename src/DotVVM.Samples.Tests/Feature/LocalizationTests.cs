using System.Globalization;
using System.Threading;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class LocalizationTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_Localization_Localization()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Localization_Localization);

                ChangeAndTestLocalization(browser);
            });
        }

        [Fact]
        public void Feature_Localization_Localization_FormatString()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Localization_Localization_FormatString);
                var cultureElement = browser.First("#culture");

                AssertUI.InnerText(cultureElement, s => !string.IsNullOrWhiteSpace(s), "Text is empty and should not be! (Missing current culture code!)");

                var culture = cultureElement.GetText();
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
                var value = 12.3456;

                //not supported by framework
                //AssertUI.InnerTextEquals(browser.First("#HardCodedValue"), value.ToString("#0.00"));

                //supported
                AssertUI.InnerTextEquals(browser.First("#HardCodedValueInBinding"), value.ToString("#0.00"));
            });
        }

        private static void ChangeAndTestLocalization(IBrowserWrapper browser)
        {
            AssertUI.InnerTextEquals(browser.First("p"), "This comes from resource file!", false, true);
            // change language
            browser.Last("a").Click();
            AssertUI.InnerTextEquals(browser.First("p"), "Tohle pochází z resource souboru!", false, true);
        }

        [Fact]
        public void Feature_Localization_Localization_NestedPage_Type()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Localization_Localization_NestedPage_Type);

                AssertUI.InnerTextEquals(browser.First("#masterPage"), "Master page", false);
                AssertUI.InnerTextEquals(browser.First("#fromLocalizationFile1"), "This comes from resource file!", false);
                AssertUI.InnerTextEquals(browser.First("#fromLocalizationFile2"), "Nested page title", false);

                // change language
                browser.Last("a").Click();
                AssertUI.InnerTextEquals(browser.First("#masterPage"), "Master page", false);
                AssertUI.InnerTextEquals(browser.First("#fromLocalizationFile1"), "Tohle pochází z resource souboru!", false);
                AssertUI.InnerTextEquals(browser.First("#fromLocalizationFile2"), "Nested page title", false);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_Localization_Localization_Control_Page))]
        public void Feature_Localization_Localization_Control_Page_FullNames()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Localization_Localization_Control_Page);

                Assert.Equal("Localized label for checkbox inside control",
                    browser.Driver.FindElement(By.XPath("//div[@data-ui='localization-control-bare']/label/span")).Text);
                Assert.Equal("Localized literal inside control",
                    browser.Driver.FindElement(By.XPath("//div[@data-ui='localization-control-bare']/span")).Text);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_Localization_Localization_Control_Page))]
        public void Feature_Localization_Localization_Control_Page_ImportUsed()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Localization_Localization_Control_Page);

                Assert.Equal("Localized label for checkbox inside control",
                    browser.Driver.FindElement(By.XPath("//div[@data-ui='localization-control-import']/label/span")).Text);
                Assert.Equal("Localized literal inside control",
                    browser.Driver.FindElement(By.XPath("//div[@data-ui='localization-control-import']/span")).Text);
            });
        }

        [Fact]
        public void Feature_Localization_Globalize()
        {
            void CheckForm(IBrowserWrapper browser) {
                var oldSelectMethod = browser.SelectMethod;
                browser.SelectMethod = SelectByDataUi;

                browser.Single("button-hello").Click();
                AssertUI.TextEquals(browser.Single("span-hello"), "Hello");

                browser.Single("textbox-parse").SendKeys("42");
                browser.Single("button-parse").Click();
                AssertUI.TextEquals(browser.Single("span-parse"), "42");

                browser.Single("textbox-multiplyA").Clear().SendKeys("6");
                browser.Single("textbox-multiplyB").Clear().SendKeys("-7");
                browser.Single("button-multiply").Click();
                AssertUI.TextEquals(browser.Single("span-multiply"), "-42");

                AssertUI.TextEquals(browser.Single("postback-counter"), "3");

                browser.SelectMethod = oldSelectMethod;
            }

            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Localization_Globalize);
                browser.WaitUntilDotvvmInited();
                CheckForm(browser);

                browser.Single("switch-czech", SelectByDataUi).Click();
                browser.WaitUntilDotvvmInited();
                CheckForm(browser);
            });
        }

        public LocalizationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
