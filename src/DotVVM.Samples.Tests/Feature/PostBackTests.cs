using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Selenium.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Threading;
using DotVVM.Testing.Abstractions;


namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class PostBackTests : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_PostBack_PostbackUpdate()
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
        public void Feature_PostBack_PostbackUpdateRepeater()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostBack_PostbackUpdateRepeater);

                // enter the text and click the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "test");
                browser.Click("input[type=button]");
                browser.Wait();

                // check the inner text of generated items
                browser.FindElements("p.item").ThrowIfDifferentCountThan(5).ForEach(e => e.CheckIfInnerTextEquals("test"));
                
                // change the text and client the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "xxx");
                browser.Click("input[type=button]");
                browser.Wait();

                browser.FindElements("p.item").ThrowIfDifferentCountThan(5).ForEach(e => e.CheckIfInnerTextEquals("xxx"));
            });
        }

        [TestMethod]
        public void Feature_PostBack_PostBackHandlers()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostBack_PostBackHandlers);
                browser.Wait();
                var index = browser.First("[data-ui=\"command-index\"]");

                // confirm first
                browser.ElementAt("input[type=button]", 0).Click();
                browser.CheckIfAlertTextEquals("Confirmation 1");
                browser.ConfirmAlert();
                browser.Wait();
                index.CheckIfInnerTextEquals("1");

                // cancel second
                browser.ElementAt("input[type=button]", 1).Click();
                browser.CheckIfAlertTextEquals("Confirmation 1");
                browser.ConfirmAlert();
                browser.Wait();

                browser.CheckIfAlertTextEquals("Confirmation 2");
                browser.GetAlert().Dismiss();
                browser.Wait();
                index.CheckIfInnerTextEquals("1");
                // confirm second
                browser.ElementAt("input[type=button]", 1).Click();
                browser.CheckIfAlertTextEquals("Confirmation 1");
                browser.ConfirmAlert();
                browser.Wait();
                browser.CheckIfAlertTextEquals("Confirmation 2");
                browser.ConfirmAlert();
                browser.Wait();
                index.CheckIfInnerTextEquals("2");

                // confirm third
                browser.ElementAt("input[type=button]", 2).Click();
                Assert.IsFalse(browser.HasAlert());
                browser.Wait();
                index.CheckIfInnerTextEquals("3");

                // confirm fourth
                browser.ElementAt("input[type=button]", 3).Click();
                browser.CheckIfAlertTextEquals("Generated 1");
                browser.ConfirmAlert();
                browser.Wait();
                index.CheckIfInnerTextEquals("4");

                // confirm fifth
                browser.ElementAt("input[type=button]", 4).Click();
                browser.CheckIfAlertTextEquals("Generated 2");
                browser.ConfirmAlert();
                browser.Wait();
                index.CheckIfInnerTextEquals("5");

                // confirm conditional
                browser.ElementAt("input[type=button]", 5).Click();
                Assert.IsFalse(browser.HasAlert());
                browser.Wait();
                index.CheckIfInnerTextEquals("6");

                browser.First("input[type=checkbox]").Click();

                browser.ElementAt("input[type=button]", 5).Click();
                browser.CheckIfAlertTextEquals("Conditional 1");
                browser.ConfirmAlert();
                browser.Wait();
                index.CheckIfInnerTextEquals("6");

                browser.First("input[type=checkbox]").Click();

                browser.ElementAt("input[type=button]", 5).Click();
                Assert.IsFalse(browser.HasAlert());
                browser.Wait();
                index.CheckIfInnerTextEquals("6");

                browser.First("input[type=checkbox]").Click();

                browser.ElementAt("input[type=button]", 5).Click();
                browser.CheckIfAlertTextEquals("Conditional 1");
                browser.ConfirmAlert();
                browser.Wait();
                index.CheckIfInnerTextEquals("6");
            });
        }
    }
}