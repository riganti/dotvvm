using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ResourcesTests : SeleniumTestBase
    {

        [TestMethod]
        public void CdnUnavailableResourceLoad()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Resources_CdnUnavailableResourceLoad);
                browser.Wait();

                // verify that if CDN is not available, local script loads
                browser.CheckIfAlertTextEquals("javascript resource loaded!");
            });
        }


        [TestMethod]
        public void CdnScriptPriority()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Resources_CdnScriptPriority);
                browser.Wait();

                // verify that if CDN is available, local script doesn't load
                browser.CheckIfAlertTextEquals("javascript resource loaded!");
            });
        }

    }
}
