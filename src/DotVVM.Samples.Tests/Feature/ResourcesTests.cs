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
    public class ResourcesTests : SeleniumTest
    {

        [TestMethod]
        public void CdnUnavailableResourceLoad()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Resources_CdnUnavailableResourceLoad);

                // verify that if CDN is not available, local script loads
                browser.WaitFor(browser.HasAlert, 5000, "An alert was expected to open!");
                browser.CheckIfAlertTextEquals("javascript resource loaded!");
                browser.ConfirmAlert();
            });
        }


        [TestMethod]
        public void CdnScriptPriority()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Resources_CdnScriptPriority);

                // verify that if CDN is not available, local script loads
                browser.WaitFor(browser.HasAlert, 5000, "An alert was expected to open!");
                browser.CheckIfAlertTextEquals("javascript resource loaded!");
                browser.ConfirmAlert();
            });
        }

    }
}
