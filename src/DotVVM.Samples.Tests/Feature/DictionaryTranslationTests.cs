using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class DictionaryTranslationTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_DictionaryTranslation_Clear()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_DictionaryIndexerTranslation);

                // Clear dictionary
                var inputs = browser.FindElements("input");
                inputs.ElementAt(4).Click();

                WaitForExecutor.WaitFor(() => {
                    var spans = browser.FindElements("span");
                    AssertUI.Text(spans.FirstOrDefault(), s => !s.Contains("KEY: "), waitForOptions: WaitForOptions.Disabled);
                    AssertUI.Text(spans.ElementAt(1), s => !s.Contains("VAL: "), waitForOptions: WaitForOptions.Disabled);
                });

            });
        }

        [Fact]
        public void Feature_DictionaryTranslation_ContainsKey()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_DictionaryIndexerTranslation);

                var inputs = browser.FindElements("input");
                inputs.FirstOrDefault().SendKeys("key1");
                inputs.ElementAt(2).Click();
                AssertUI.TextEquals(inputs.ElementAt(2), "true");

                inputs.FirstOrDefault().Clear().SendKeys("key123");
                inputs.ElementAt(2).Click();
                AssertUI.TextEquals(inputs.ElementAt(2), "false");
            });
        }

        [Fact]
        public void Feature_DictionaryTranslation_Remove()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_DictionaryIndexerTranslation);

                var inputs = browser.FindElements("input");
                inputs.FirstOrDefault().SendKeys("key1");
                inputs.ElementAt(5).Click();

                var spans = browser.FindElements("span");
                AssertUI.TextEquals(spans.FirstOrDefault(), "KEY: \"key2\"");
                AssertUI.TextEquals(spans.ElementAt(1), "VAL: \"value2\"");
            });
        }

        [Fact]
        public void Feature_DictionaryTranslation_GetItem()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_DictionaryIndexerTranslation);

                var spans = browser.FindElements("span");
                AssertUI.TextEquals(spans.FirstOrDefault(), "KEY: \"key1\"");
                AssertUI.TextEquals(spans.ElementAt(1), "VAL: \"value1\"");
                AssertUI.TextEquals(spans.ElementAt(2), "KEY: \"key2\"");
                AssertUI.TextEquals(spans.ElementAt(3), "VAL: \"value2\"");
            });
        }

        [Fact]
        public void Feature_DictionaryTranslation_SetItem()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_DictionaryIndexerTranslation);

                // Change value
                var inputs = browser.FindElements("input");
                inputs.FirstOrDefault().SendKeys("key1");
                inputs.ElementAt(1).SendKeys("newValue");
                inputs.ElementAt(3).Click();

                var spans = browser.FindElements("span");
                AssertUI.TextEquals(spans.FirstOrDefault(), "KEY: \"key1\"");
                AssertUI.TextEquals(spans.ElementAt(1), "VAL: \"newValue\"");
            });
        }

        [Fact]
        public void Feature_DictionaryTranslation_AddKeyValue()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_DictionaryIndexerTranslation);
                browser.WaitUntilDotvvmInited();

                // Create new key-value
                var inputs = browser.FindElements("input");
                inputs.FirstOrDefault().SendKeys("key123");
                inputs.ElementAt(1).SendKeys("value123");
                inputs.ElementAt(3).Click();

                var spans = browser.FindElements("span");
                var spansTexts = spans.Select(s => s.GetText()).ToList();
                AssertUI.TextEquals(spans.ElementAt(4), "KEY: \"key123\"");
                AssertUI.TextEquals(spans.ElementAt(5), "VAL: \"value123\"");
            });
        }

        [Fact]
        public void Feature_DictionaryTranslation_AddKeyValue_ThenSetItem()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_DictionaryIndexerTranslation);

                // Create new key-value
                var inputs = browser.FindElements("input");
                inputs.FirstOrDefault().SendKeys("key123");
                inputs.ElementAt(1).SendKeys("value123");
                inputs.ElementAt(3).Click();

                WaitForExecutor.WaitFor(() => {
                    var spans = browser.FindElements("span");
                    AssertUI.TextEquals(spans.ElementAt(4), "KEY: \"key123\"", waitForOptions: WaitForOptions.Disabled);
                    AssertUI.TextEquals(spans.ElementAt(5), "VAL: \"value123\"", waitForOptions: WaitForOptions.Disabled);
                });

                // Change value
                inputs.FirstOrDefault().Clear().SendKeys("key123");
                inputs.ElementAt(1).Clear().SendKeys("changed-value123");
                inputs.ElementAt(3).Click();
                WaitForExecutor.WaitFor(() => {
                    var spans = browser.FindElements("span");
                    AssertUI.TextEquals(spans.ElementAt(4), "KEY: \"key123\"", waitForOptions: WaitForOptions.Disabled);
                    AssertUI.TextEquals(spans.ElementAt(5), "VAL: \"changed-value123\"", waitForOptions: WaitForOptions.Disabled);
                });
            });
        }

        public DictionaryTranslationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
