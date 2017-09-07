using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ServerSideStylesTests : SeleniumTest
    {
        [TestMethod]
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
        public void Feature_ServerSideStyles_HtmlControlNoAttributes()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles);
                browser.First("customTagName[id=htmlControlNoAttr]").CheckAttribute("append", "Attribute changed");
                browser.First("customTagName[id=htmlControlNoAttr]").CheckAttribute("noAppend", "Attribute changed");
            });
        }

        [TestMethod]
        public void Feature_ServerSideStyles_HtmlControlWithAttributes()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles);
                browser.First("customTagName[id=htmlControlWithAttr]").CheckAttribute("append", "Attribute changed");
                browser.First("customTagName[id=htmlControlWithAttr]").CheckAttribute("noAppend", "Default attribute");
            });
        }


        [TestMethod]
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
