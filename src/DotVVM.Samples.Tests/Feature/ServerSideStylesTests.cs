using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ServerSideStylesTests : AppSeleniumTest
    {
        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles))]
        public void Feature_ServerSideStyles_DotvvmControlNoAttributes()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles);
                browser.First("input[id=dotvvmControlNoAttr]").CheckClassAttribute("Class changed");
                browser.First("input[id=dotvvmControlNoAttr]").CheckIfTextEquals("Text changed");
            });
        }

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles))]
        public void Feature_ServerSideStyles_DotvvmControlWithAttributes()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles);
                browser.First("input[id=dotvvmControlWithAttr]").CheckClassAttribute("Class changed");
                browser.First("input[id=dotvvmControlWithAttr]").CheckIfTextEquals("Default text");
            });
        }

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles))]
        public void Feature_ServerSideStyles_HtmlControlNoAttributes()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles);
                browser.First("customTagName[id=htmlControlNoAttr]").CheckAttribute("ignore", "Attribute ignored");
                browser.First("customTagName[id=htmlControlNoAttr]").CheckAttribute("append", "Attribute appended");
                browser.First("customTagName[id=htmlControlNoAttr]").CheckAttribute("overwrite", "Attribute changed");
                browser.First("customTagName[id=htmlControlNoAttr]").CheckAttribute("class", "new-class");
            });
        }

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles))]
        public void Feature_ServerSideStyles_HtmlControlWithAttributes()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles);
                browser.First("customTagName[id=htmlControlWithAttr]").CheckAttribute("ignore", "Default attribute");
                browser.First("customTagName[id=htmlControlWithAttr]").CheckAttribute("append", "Default attribute;Attribute appended");
                browser.First("customTagName[id=htmlControlWithAttr]").CheckAttribute("overwrite", "Attribute changed");
                browser.First("customTagName[id=htmlControlWithAttr]").CheckAttribute("class", "default-class new-class");
            });
        }


        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_DotvvmProperties))]
        public void Feature_ServerSideStyles_DotvvmControlProperties()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_DotvvmProperties);
                browser.First("input[id=dotvvmControlWithAttr]").CheckAttribute("customAttr", "Default value");
                browser.First("input[id=dotvvmControlNoAttr]").CheckAttribute("customAttr", "Custom property changed");
            });
        }

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_DotvvmProperties))]
        public void Feature_ServerSideStyles_DerivedMatcher()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_DotvvmProperties);
                browser.First("input[id=dotvvmControlWithAttr]").CheckIfHasAttribute("derivedAttr");
                browser.First("input[id=dotvvmControlNoAttr]").CheckIfHasAttribute("derivedAttr");
                browser.First("input[id=derivedControl]").CheckIfHasNotAttribute("derivedAttr");
            });
        }

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_DotvvmProperties))]
        public void Feature_ServerSideStyles_Matcher()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_DotvvmProperties);
                browser.First("input[id=dotvvmControlWithAttr]").CheckIfHasAttribute("addedAttr");
                browser.First("input[id=dotvvmControlNoAttr]").CheckIfHasNotAttribute("addedAttr");
                browser.First("input[id=derivedControl]").CheckIfHasAttribute("addedAttr");
            });
        }

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_DirectoryStyle_ServerSideStyles))]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_NoDirectoryStyle_ServerSideStyles))]
        public void Feature_ServerSideStyles_Directory()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_DirectoryStyle_ServerSideStyles);
                browser.First("input[id=dotvvmControl]").CheckAttribute("directory", "matching");
                browser.First("customtagname[id=htmlControl]").CheckAttribute("directory", "matching");
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_NoDirectoryStyle_ServerSideStyles);
                browser.First("input[id=dotvvmControl]").CheckIfHasNotAttribute("directory");
                browser.First("customtagname[id=htmlControl]").CheckIfHasNotAttribute("directory");
            });
        }

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_MatchingViewModel))]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles))]
        public void Feature_ServerSideStyles_DataContexts()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles_MatchingViewModel);
                browser.First("customDataContextTag[id=matchingDataContextAndRoot]").CheckAttribute("dataContextCheck", "matching");
                browser.First("customDataContextTag[id=matchingDataContextAndRoot]").CheckAttribute("rootDataContextCheck", "matching");
                browser.First("customDataContextTag[id=matchingRoot]").CheckAttribute("rootDataContextCheck", "matching");
                browser.First("customDataContextTag[id=matchingRoot]").CheckIfHasNotAttribute("dataContextCheck");
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles);
                browser.First("customDataContextTag[id=nonMatchingDataContextAndRoot]").CheckIfHasNotAttribute("rootDataContextCheck");
                browser.First("customDataContextTag[id=nonMatchingDataContextAndRoot]").CheckIfHasNotAttribute("dataContextCheck");
            });
        }
    }
}
