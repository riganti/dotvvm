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
    public class RenderSettingsModeServerPropertyTest : SeleniumTestBase
    {
        [TestMethod]
        public void Features_RenderSettingsModeServerProperty()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_RenderSettingsModeServerProperty);

                // ensure month names are rendered on the server
                browser.First("table tr td").FindElements("span").ThrowIfDifferentCountThan(0);
                
                // fill textboxes
                browser.SendKeys("input[type=text]", "1");

                //browser.FindElements("input[type=text]").
                browser.Click("input[type=button]");
                browser.Wait();


                // validate result
                browser.Last("span").CheckIfInnerTextEquals("12", false, true);
            });
        }
    }
}
