using System;
using System.IO;
using System.Text.RegularExpressions;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    [Trait("Category", "aspnetcore-latest-only")]
    public class StaticAssetsTests : AppSeleniumTest
    {
        public StaticAssetsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_StaticAssets_StaticAssets))]
        [InlineData("[data-ui='test-link']", "href", "/Content", "staticAssetTest", "css")]
        [InlineData("[data-ui='test-img']", "src", "/Content", "staticAssetTestImage", "svg")]
        [InlineData("[data-ui='script-test']", "src", "/Scripts", "staticAssetTest", "js")]
        public void Feature_StaticAssets_ResourcesContainHash(string selector, string attribute, string expectedDirectory, string expectedName, string extension)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticAssets_StaticAssets);
                browser.WaitUntilDotvvmInited();

                var element = browser.Single(selector);
                var url = element.GetAttribute(attribute);
                var path = new Uri(url).PathAndQuery;

                Assert.DoesNotContain("~/", url, StringComparison.Ordinal);
                Assert.StartsWith(expectedDirectory + "/", path, StringComparison.Ordinal);

                var regex = new Regex($@"/{Regex.Escape(expectedName)}\.[a-zA-Z0-9]+\.{Regex.Escape(extension)}$");
                Assert.Matches(regex, url);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_StaticAssets_StaticAssets))]
        public void Feature_StaticAssets_ScriptLoads()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticAssets_StaticAssets);
                browser.WaitUntilDotvvmInited();

                AssertUI.TextEquals(browser.Single("[data-ui='script-loaded-result']"), "true");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_StaticAssets_StaticAssets))]
        public void Feature_StaticAssets_CssApplied()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticAssets_StaticAssets);
                browser.WaitUntilDotvvmInited();

                var styledElement = browser.Single("[data-ui='styled-element']");
                var color = styledElement.WebElement.GetCssValue("color");
                var fontWeight = styledElement.WebElement.GetCssValue("font-weight");

                Assert.True(color.Trim() is "green" or "rgb(0, 128, 0)", $"Expected green color, but got: {color}");

                Assert.True(fontWeight is "700" or "bold", $"Expected bold font weight, but got: {fontWeight}");
            });
        }
    }
}
