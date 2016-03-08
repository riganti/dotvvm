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

                browser.FindElements("br").ThrowIfDifferentCountThan(14);

                // change number of lines and click the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "5");
                browser.Click("input[type=button]");

                browser.FindElements("br").ThrowIfDifferentCountThan(4);
            });
        }

        [TestMethod]
        public void Feature_PostBackUpdateRepeater()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostBack_PostbackUpdateRepeater);

                // enter the text and click the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "test");
                browser.Click("input[type=button]");

                // check the inner text of generated items
                browser.FindElements("p.item").ThrowIfDifferentCountThan(5).ForEach(e => e.CheckIfInnerTextEquals("test"));
                
                // change the text and client the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "xxx");
                browser.Click("input[type=button]");

                browser.FindElements("p.item").ThrowIfDifferentCountThan(5).ForEach(e => e.CheckIfInnerTextEquals("xxx"));
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
                browser.CheckIfAlertTextEquals("Confirmation 1");
                browser.ConfirmAlert();
                browser.Wait();
                browser.Last("span").CheckIfInnerTextEquals("1");

                // cancel second
                browser.ElementAt("input[type=button]", 1).Click();
                browser.CheckIfAlertTextEquals("Confirmation 1");
                browser.ConfirmAlert();
                browser.Wait();

                browser.CheckIfAlertTextEquals("Confirmation 2");
                browser.GetAlert().Dismiss();
                browser.Wait();
                browser.FindElements("span").Last().CheckIfInnerTextEquals("1");
                // confirm second
                browser.ElementAt("input[type=button]", 1).Click();
                browser.CheckIfAlertTextEquals("Confirmation 1");
                browser.ConfirmAlert();
                browser.Wait();
                browser.CheckIfAlertTextEquals("Confirmation 2");
                browser.ConfirmAlert();
                browser.Wait();
                browser.Last("span").CheckIfInnerTextEquals("2");

                // confirm third
                browser.ElementAt("input[type=button]", 2).Click();
                Assert.IsFalse(browser.HasAlert());
                browser.Wait();
                browser.Last("span").CheckIfInnerTextEquals("3");

                // confirm fourth
                browser.ElementAt("input[type=button]", 3).Click();
                browser.CheckIfAlertTextEquals("Generated 1");
                browser.ConfirmAlert();
                browser.Wait();
                browser.Last("span").CheckIfInnerTextEquals("4");

                // confirm fifth
                browser.ElementAt("input[type=button]", 4).Click();
                browser.CheckIfAlertTextEquals("Generated 2");
                browser.ConfirmAlert();
                browser.Wait();
                browser.Last("span").CheckIfInnerTextEquals("5");

                // confirm conditional
                browser.ElementAt("input[type=button]", 5).Click();
                Assert.IsFalse(browser.HasAlert());
                browser.Wait();
                browser.Last("span").CheckIfInnerTextEquals("6");

                browser.First("input[type=checkbox]").Click();

                browser.ElementAt("input[type=button]", 5).Click();
                browser.CheckIfAlertTextEquals("Conditional 1");
                browser.ConfirmAlert();
                browser.Wait();
                browser.Last("span").CheckIfInnerTextEquals("6");

                browser.First("input[type=checkbox]").Click();

                browser.ElementAt("input[type=button]", 5).Click();
                Assert.IsFalse(browser.HasAlert());
                browser.Wait();
                browser.Last("span").CheckIfInnerTextEquals("6");

                browser.First("input[type=checkbox]").Click();

                browser.ElementAt("input[type=button]", 5).Click();
                browser.CheckIfAlertTextEquals("Conditional 1");
                browser.ConfirmAlert();
                browser.Wait();
                browser.Last("span").CheckIfInnerTextEquals("6");
            });
        }
    }
}