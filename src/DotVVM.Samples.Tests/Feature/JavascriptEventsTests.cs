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
                //TODO: Change to CheckIfAlertTextContains
                Assert.AreEqual("init", browser.GetAlertText());
                browser.ConfirmAlert();

                // postback alerts
                browser.ElementAt("input[type=button]", 0).Click().Wait();
                //TODO: Change to CheckIfAlertTextContains
                Assert.AreEqual("beforePostback", browser.GetAlertText());
                browser.ConfirmAlert();
                browser.Wait();
                //TODO: Change to CheckIfAlertTextContains
                Assert.AreEqual("afterPostback", browser.GetAlertText());
                browser.ConfirmAlert();

                // error alerts
                browser.ElementAt("input[type=button]", 1).Click().Wait();
                //TODO: Change to CheckIfAlertTextContains
                Assert.AreEqual("beforePostback", browser.GetAlertText());
                browser.ConfirmAlert();
                browser.Wait();
                //TODO: Change to CheckIfAlertTextContains
                Assert.AreEqual("custom error handler", browser.GetAlertText());
                browser.ConfirmAlert();
            });
        }
    }
}