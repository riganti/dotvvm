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
        public void Feature_ServerSideStyles_DotvvmContolNoAttributes()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles);
                browser.First("input[id=dotvvmControlNoAttr]").CheckClassAttribute("Class changed");
                browser.First("input[id=dotvvmControlNoAttr]").CheckIfTextEquals("Text changed");
            });
        }

        [TestMethod]
        public void Feature_ServerSideStyles_DotvvmContolWithAttributes()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerSideStyles_ServerSideStyles);
                browser.First("input[id=dotvvmControlWithAttr]").CheckClassAttribute("Class changed");
                browser.First("input[id=dotvvmControlWithAttr]").CheckIfTextEquals("Default text");
            });
        }

        [TestMethod]
        public void Feature_ServerSideStyles_HtmlContolNoAttributes()
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
    }
}
