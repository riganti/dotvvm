
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Testing.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class HtmlTagTests : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_HtmlTag_NonPairHtmlTag()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_HtmlTag_NonPairHtmlTag);

                browser.ElementAt("div", 0).FindElements("hr").ThrowIfDifferentCountThan(2);
                browser.ElementAt("div", 1).FindElements("hr").ThrowIfDifferentCountThan(1);
                browser.ElementAt("div", 2).First("span").CheckIfInnerTextEquals("Hello");
               
            });
        }
    }
}
