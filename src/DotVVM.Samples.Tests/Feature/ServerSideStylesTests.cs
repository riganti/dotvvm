using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class ServerSideStylesTests : AppSeleniumTest
    {
        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles))]
        public void Feature_ServerSideStyles_DotvvmControlNoAttributes()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles);
                AssertUI.HasClass(browser.First("input[id=dotvvmControlNoAttr]"), "Class changed");
                AssertUI.TextEquals(browser.First("input[id=dotvvmControlNoAttr]"), "Text changed");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles))]
        public void Feature_ServerSideStyles_DotvvmControlWithAttributes()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles);
                AssertUI.HasClass(browser.First("input[id=dotvvmControlWithAttr]"), "Class changed");
                AssertUI.TextEquals(browser.First("input[id=dotvvmControlWithAttr]"), "Default text");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles))]
        public void Feature_ServerSideStyles_HtmlControlNoAttributes()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles);
                AssertUI.Attribute(browser.First("customTagName[id=htmlControlNoAttr]"), "ignore", "Attribute ignored");
                AssertUI.Attribute(browser.First("customTagName[id=htmlControlNoAttr]"), "append", "Attribute appended");
                AssertUI.Attribute(browser.First("customTagName[id=htmlControlNoAttr]"), "overwrite", "Attribute changed");
                AssertUI.Attribute(browser.First("customTagName[id=htmlControlNoAttr]"), "class", "new-class");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles))]
        public void Feature_ServerSideStyles_HtmlControlWithAttributes()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles);
                AssertUI.Attribute(browser.First("customTagName[id=htmlControlWithAttr]"), "ignore", "Default attribute");
                AssertUI.Attribute(browser.First("customTagName[id=htmlControlWithAttr]"), "append", "Default attribute;Attribute appended");
                AssertUI.Attribute(browser.First("customTagName[id=htmlControlWithAttr]"), "overwrite", "Attribute changed");
                AssertUI.Attribute(browser.First("customTagName[id=htmlControlWithAttr]"), "class", "default-class new-class");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_DotvvmProperties))]
        public void Feature_ServerSideStyles_DotvvmControlProperties()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_DotvvmProperties);
                AssertUI.Attribute(browser.First("input[id=dotvvmControlWithAttr]"), "customAttr", "Default value");
                AssertUI.Attribute(browser.First("input[id=dotvvmControlNoAttr]"), "customAttr", "Custom property changed");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_DotvvmProperties))]
        public void Feature_ServerSideStyles_DerivedMatcher()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_DotvvmProperties);
                AssertUI.HasAttribute(browser.First("input[id=dotvvmControlWithAttr]"), "derivedAttr");
                AssertUI.HasAttribute(browser.First("input[id=dotvvmControlNoAttr]"), "derivedAttr");
                AssertUI.HasNotAttribute(browser.First("input[id=derivedControl]"), "derivedAttr");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_DotvvmProperties))]
        public void Feature_ServerSideStyles_Matcher()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_DotvvmProperties);
                AssertUI.HasAttribute(browser.First("input[id=dotvvmControlWithAttr]"), "addedAttr");
                AssertUI.HasNotAttribute(browser.First("input[id=dotvvmControlNoAttr]"), "addedAttr");
                AssertUI.HasAttribute(browser.First("input[id=derivedControl]"), "addedAttr");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_DirectoryStyle_ServerSideStyles))]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_NoDirectoryStyle_ServerSideStyles))]
        public void Feature_ServerSideStyles_Directory()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_DirectoryStyle_ServerSideStyles);
                AssertUI.Attribute(browser.First("input[id=dotvvmControl]"), "directory", "matching");
                AssertUI.Attribute(browser.First("customtagname[id=htmlControl]"), "directory", "matching");
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_NoDirectoryStyle_ServerSideStyles);
                AssertUI.HasNotAttribute(browser.First("input[id=dotvvmControl]"), "directory");
                AssertUI.HasNotAttribute(browser.First("customtagname[id=htmlControl]"), "directory");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_MatchingViewModel))]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles))]
        public void Feature_ServerSideStyles_DataContexts()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_MatchingViewModel);
                AssertUI.Attribute(browser.First("customDataContextTag[id=matchingDataContextAndRoot]"), "dataContextCheck", "matching");
                AssertUI.Attribute(browser.First("customDataContextTag[id=matchingDataContextAndRoot]"), "rootDataContextCheck", "matching");
                AssertUI.Attribute(browser.First("customDataContextTag[id=matchingRoot]"), "rootDataContextCheck", "matching");
                AssertUI.HasNotAttribute(browser.First("customDataContextTag[id=matchingRoot]"), "dataContextCheck");
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles);
                AssertUI.HasNotAttribute(browser.First("customDataContextTag[id=nonMatchingDataContextAndRoot]"), "rootDataContextCheck");
                AssertUI.HasNotAttribute(browser.First("customDataContextTag[id=nonMatchingDataContextAndRoot]"), "dataContextCheck");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_ControlProperties))]
        public void Feature_ServerSideStyles_ControlProperties()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_ControlProperties);

                browser.First("input[server-side-style-attribute]").Click();
                AssertUI.AlertTextEquals(browser, "ConfirmPostBackHandler Content");
            });
        }

        public ServerSideStylesTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
