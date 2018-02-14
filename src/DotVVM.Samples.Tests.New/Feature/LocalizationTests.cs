using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.New;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;
using Xunit.Abstractions;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

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

                Assert.AreEqual("Localized label for checkbox inside control",
                    browser.Driver.FindElement(By.XPath("//div[@data-ui='localization-control-bare']/label/span")).Text);
                Assert.AreEqual("Localized literal inside control",
                    browser.Driver.FindElement(By.XPath("//div[@data-ui='localization-control-bare']/span")).Text);

            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_Localization_Localization_Control_Page))]
        public void Feature_Localization_Localization_Control_Page_ImportUsed()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Localization_Localization_Control_Page);

                Assert.AreEqual("Localized label for checkbox inside control",
                    browser.Driver.FindElement(By.XPath("//div[@data-ui='localization-control-import']/label/span")).Text);
                Assert.AreEqual("Localized literal inside control",
                    browser.Driver.FindElement(By.XPath("//div[@data-ui='localization-control-import']/span")).Text);

            });
        }

        public LocalizationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
