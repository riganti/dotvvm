using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
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
                var inputs = browser.FindElements("input").Take(6);
                inputs.Skip(4).First().Click();

                var spans = browser.FindElements("span");
                Assert.DoesNotContain("KEY: ", spans.First().GetText());
                Assert.DoesNotContain("VAL: ", spans.Skip(1).First().GetText());
            });
        }

        [Fact]
        public void Feature_DictionaryTranslation_ContainsKey()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_DictionaryIndexerTranslation);

                var inputs = browser.FindElements("input").Take(6);
                inputs.First().SendKeys("key1");
                inputs.Skip(2).First().Click();
                Assert.Equal("true", inputs.Skip(2).First().GetText());

                inputs.First().Clear().SendKeys("key123");
                inputs.Skip(2).First().Click();
                Assert.Equal("false", inputs.Skip(2).First().GetText());
            });
        }

        [Fact]
        public void Feature_DictionaryTranslation_Remove()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_DictionaryIndexerTranslation);

                var inputs = browser.FindElements("input").Take(6);
                inputs.First().SendKeys("key1");
                inputs.Skip(5).First().Click();

                var spans = browser.FindElements("span");
                Assert.Equal("KEY: \"key2\"", spans.First().GetText());
                Assert.Equal("VAL: \"value2\"", spans.Skip(1).First().GetText());
            });
        }

        [Fact]
        public void Feature_DictionaryTranslation_GetItem()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_DictionaryIndexerTranslation);

                var spans = browser.FindElements("span");
                Assert.Equal("KEY: \"key1\"", spans.First().GetText());
                Assert.Equal("VAL: \"value1\"", spans.Skip(1).First().GetText());
                Assert.Equal("KEY: \"key2\"", spans.Skip(2).First().GetText());
                Assert.Equal("VAL: \"value2\"", spans.Skip(3).First().GetText());
            });
        }

        [Fact]
        public void Feature_DictionaryTranslation_SetItem()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_DictionaryIndexerTranslation);

                // Change value
                var inputs = browser.FindElements("input").Take(6);
                inputs.First().SendKeys("key1");
                inputs.Skip(1).First().SendKeys("newValue");
                inputs.Skip(3).First().Click();

                var spans = browser.FindElements("span");
                Assert.Equal("KEY: \"key1\"", spans.First().GetText());
                Assert.Equal("VAL: \"newValue\"", spans.Skip(1).First().GetText());
            });
        }

        [Fact]
        public void Feature_DictionaryTranslation_AddKeyValue()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_DictionaryIndexerTranslation);

                // Create new key-value
                var inputs = browser.FindElements("input").Take(6);
                inputs.First().SendKeys("key123");
                inputs.Skip(1).First().SendKeys("value123");
                inputs.Skip(3).First().Click();

                var spans = browser.FindElements("span");
                Assert.Equal("KEY: \"key123\"", spans.Skip(4).First().GetText());
                Assert.Equal("VAL: \"value123\"", spans.Skip(5).First().GetText());
            });
        }

        [Fact]
        public void Feature_DictionaryTranslation_AddKeyValue_ThenSetItem()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_DictionaryIndexerTranslation);

                // Create new key-value
                var inputs = browser.FindElements("input").Take(6);
                inputs.First().SendKeys("key123");
                inputs.Skip(1).First().SendKeys("value123");
                inputs.Skip(3).First().Click();

                var spans = browser.FindElements("span");
                Assert.Equal("KEY: \"key123\"", spans.Skip(4).First().GetText());
                Assert.Equal("VAL: \"value123\"", spans.Skip(5).First().GetText());

                // Change value
                inputs.First().Clear().SendKeys("key123");
                inputs.Skip(1).First().Clear().SendKeys("changed-value123");
                inputs.Skip(3).First().Click();

                Assert.Equal("KEY: \"key123\"", spans.Skip(4).First().GetText());
                Assert.Equal("VAL: \"changed-value123\"", spans.Skip(5).First().GetText());
            });
        }

        public DictionaryTranslationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
