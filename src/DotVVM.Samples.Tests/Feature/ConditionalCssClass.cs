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
    public class ConditionalCssClass : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_ConditionalCssClasses_ConditionalCssClasses()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ConditionalCssClasses_ConditionalCssClasses);

                browser.First("div").CheckIfHasNotClass("italic");
                browser.First("input[type=button][value=\"Switch Italic\"]").Click();
                browser.First("div").CheckIfHasClass("italic");

                browser.First("div").CheckIfHasNotClass("bordered");
                browser.First("input[type=button][value=\"Switch Bordered\"]").Click();
                browser.First("div").CheckIfHasClass("bordered");

                browser.First("div").CheckIfHasNotClass("blue");
                browser.First("input[type=button][value=\"Switch Blue\"]").Click();
                browser.First("div").CheckIfHasClass("blue");
            });

        }
    }
}
