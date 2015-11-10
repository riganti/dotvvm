using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DotVVM.Samples.Tests.Control
{
    public abstract class RedirectTest : SeleniumTestBase
    {
        private const int WaitTime = 1200;

        [TestMethod]
        public void Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("ControlSamples/Redirect");
                browser.Wait(WaitTime);

                var originalUrl = browser.CurrentUrl;
                Assert.IsTrue(originalUrl.Contains("?time="));

                // click the button
                browser.First("input[type=button]").Click();
                browser.Wait(WaitTime);

                Assert.IsTrue(originalUrl.Contains("?time="));
                Assert.AreNotEqual(originalUrl, browser.CurrentUrl);
            });
        }
    }
}