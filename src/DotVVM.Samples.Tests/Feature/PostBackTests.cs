using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Threading;
using Dotvvm.Samples.Tests;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class PostBackTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_PostBackUpdate()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostBack_PostbackUpdate);

                // enter number of lines and click the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "15");
                browser.Click("input[type=button]");
                browser.Wait();

                browser.FindElements("br").ThrowIfDifferentCountThan(14);

                // change number of lines and click the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "5");
                browser.Click("input[type=button]");
                browser.Wait();

                browser.FindElements("br").ThrowIfDifferentCountThan(4);
            });
        }

        [TestMethod]
        public void Feature_PostBackHandlers()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostBack_PostBackHandlers);
                browser.Wait();

                // confirm first
                browser.ElementAt("input[type=button]", 0).Click();
                browser.Wait();
                browser.GetAlertText().Contains("Confirmation 1");
                browser.ConfirmAlert();
                browser.Wait();
                browser.FindElements("span").Last().CheckIfInnerTextEquals("1");

                // cancel second
                browser.ElementAt("input[type=button]", 1).Click();
                browser.Wait();
                browser.GetAlertText().Contains("Confirmation 1");
                browser.ConfirmAlert();
                browser.Wait();
                browser.GetAlertText().Contains("Confirmation 2");
                browser.GetAlert().Dismiss();
                browser.Wait();
                browser.FindElements("span").Last().CheckIfInnerTextEquals("1");
                // confirm second
                browser.ElementAt("input[type=button]", 1).Click();
                browser.Wait();
                browser.GetAlertText().Contains("Confirmation 1");
                browser.ConfirmAlert();
                browser.Wait();
                browser.GetAlertText().Contains("Confirmation 2");
                browser.ConfirmAlert();
                browser.Wait();
                browser.FindElements("span").Last().CheckIfInnerTextEquals("2");

                // confirm third
                browser.ElementAt("input[type=button]", 2).Click();
                browser.Wait();
                //Assert.AreEqual(null, browser.GetAlert());            // TODO: GetAlert should return null when no alert is present.
                browser.Wait();
                browser.FindElements("span").Last().CheckIfInnerTextEquals("3");

                // confirm fourth
                browser.ElementAt("input[type=button]", 3).Click();
                browser.Wait();
                browser.GetAlertText().Contains("Generated 1");
                browser.ConfirmAlert();
                browser.Wait();
                browser.FindElements("span").Last().CheckIfInnerTextEquals("4");

                // confirm fifth
                browser.ElementAt("input[type=button]", 4).Click();
                browser.Wait();
                browser.GetAlertText().Contains("Generated 2");
                browser.ConfirmAlert();
                browser.Wait();
                browser.FindElements("span").Last().CheckIfInnerTextEquals("5");
            });
        }
    }
}