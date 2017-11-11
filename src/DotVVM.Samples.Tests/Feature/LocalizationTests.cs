
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using Riganti.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Testing.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class LocalizationTests : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_Localization_Localization()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Localization_Localization);

                ChangeAndTestLocalization(browser);
            });
        }

        private static void ChangeAndTestLocalization(IBrowserWrapperFluentApi browser)
        {
            browser.First("p").CheckIfInnerTextEquals("This comes from resource file!", false, true);
            // change language
            browser.Last("a").Click();
            browser.First("p").CheckIfInnerTextEquals("Tohle pochází z resource souboru!", false, true);
        }


        [TestMethod]
        public void Feature_Localization_Localization_NestedPage_Type()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Localization_Localization_NestedPage_Type);

                browser.First("#masterPage").CheckIfInnerTextEquals("Master page", false);
                browser.First("#fromLocalizationFile1").CheckIfInnerTextEquals("This comes from resource file!", false);
                browser.First("#fromLocalizationFile2").CheckIfInnerTextEquals("Nested page title", false);

                // change language
                browser.Last("a").Click();
                browser.First("#masterPage").CheckIfInnerTextEquals("Master page", false);
                browser.First("#fromLocalizationFile1").CheckIfInnerTextEquals("Tohle pochází z resource souboru!", false);
                browser.First("#fromLocalizationFile2").CheckIfInnerTextEquals("Nested page title", false);
            });
        }

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_Localization_Localization_Control_Page))]
        public void Feature_Localization_Localization_Control_Page_FullNames()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Localization_Localization_Control_Page);

                Assert.AreEqual("Localized label for checkbox inside control", 
                    browser.Driver.FindElement(By.XPath("//div[@data-ui='localization-control-bare']/label/span")).Text);
                Assert.AreEqual("Localized literal inside control", 
                    browser.Driver.FindElement(By.XPath("//div[@data-ui='localization-control-bare']/span")).Text);

            });
        }

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_Localization_Localization_Control_Page))]
        public void Feature_Localization_Localization_Control_Page_ImportUsed()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Localization_Localization_Control_Page);

                Assert.AreEqual("Localized label for checkbox inside control",
                    browser.Driver.FindElement(By.XPath("//div[@data-ui='localization-control-import']/label/span")).Text);
                Assert.AreEqual("Localized literal inside control", 
                    browser.Driver.FindElement(By.XPath("//div[@data-ui='localization-control-import']/span")).Text);

            });
        }
    }
}
