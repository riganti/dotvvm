using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Selenium.Core;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ResourcesTests : AppSeleniumTest
    {

        [TestMethod]
        public void Feature_Resources_CdnUnavailableResourceLoad()
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
        public void Feature_Resources_CdnScriptPriority()
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

        [TestMethod]
        public void Feature_Resources_OnlineNonameResourceLoad()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Resources_OnlineNonameResourceLoad);

                //click buton
                browser.First("input[type=button]").Click();

                //check that alert showed
                browser.WaitFor(browser.HasAlert, 5000, "An alert was expected to open!");
                browser.CheckIfAlertTextEquals("resource loaded");
                browser.ConfirmAlert();
            });
        }
    }
}
