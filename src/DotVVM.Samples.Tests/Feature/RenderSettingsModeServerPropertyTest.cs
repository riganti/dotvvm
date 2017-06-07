using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class RenderSettingsModeServerPropertyTest : SeleniumTest
    {
        [TestMethod]
        public void Features_RenderSettingsModeServerProperty()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_RenderSettingsModeServer_RenderSettingModeServerProperty);

                // ensure month names are rendered on the server
                browser.FindElements("table tr td span").ThrowIfDifferentCountThan(12);
                
                // fill textboxes
                browser.SendKeys("input[type=text]", "1");

                browser.Click("input[type=button]");

                // validate result
                browser.Last("span").CheckIfInnerTextEquals("12", false, true);
            });
        }
    }
}
