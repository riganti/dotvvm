using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;
using DotVVM.Framework.Routing;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class JavascriptEventsTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_JavascriptEvents()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptEvents_JavascriptEvents);

                // init alert
                browser.Wait();
                browser.CheckIfAlertTextEquals("init");
                browser.ConfirmAlert();

                // postback alerts
                browser.ElementAt("input[type=button]", 0).Click();

                browser.CheckIfAlertTextEquals("beforePostback");
                browser.ConfirmAlert();
                browser.Wait();

                browser.CheckIfAlertTextEquals("afterPostback");
                browser.ConfirmAlert();

                // error alerts
                browser.ElementAt("input[type=button]", 1).Click();

                browser.CheckIfAlertTextEquals("beforePostback");
                browser.ConfirmAlert();
                browser.Wait();

                browser.CheckIfAlertTextEquals("custom error handler");
                browser.ConfirmAlert();
            });
        }
    }
}