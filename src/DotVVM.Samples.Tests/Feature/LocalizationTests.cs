using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class LocalizationTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_Localization()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Localization_Localization);

                //Assert.AreEqual("This comes from resource file!", browser.First("p").GetText().Trim());
                browser.First("p").CheckIfInnerTextEquals("This comes from resource file!");
                // change language
                browser.FindElements("a").Last().Click();
                browser.Wait();

                //Assert.AreEqual("Tohle pochází z resource souboru!", browser.First("p").GetText().Trim());
                browser.First("p").CheckIfInnerTextEquals("Tohle pochází z resource souboru!");
            });
        }
    }
}
