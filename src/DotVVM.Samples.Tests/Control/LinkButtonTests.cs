using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class LinkButtonTests : SeleniumTest
    {
        [TestMethod]
        public void Control_LinkButton_LinkButton()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_LinkButton_LinkButton);

                browser.First("#ButtonTextProperty").CheckTagName("a");
                browser.First("#ButtonTextBinding").CheckTagName("a");
                browser.First("#ButtonInnerText").CheckTagName("a");

                // try to click on a disabled button
                browser.Click("#EnabledLinkButton");
                browser.Wait();
                browser.Last("span").CheckIfInnerTextEquals("0");

                // enable it
                browser.Click("input[type=checkbox]");
                browser.Wait();
                browser.Click("#EnabledLinkButton");
                browser.Wait();
                browser.Last("span").CheckIfInnerTextEquals("1");

                // try to click on a disabled button again
                browser.Click("#EnabledLinkButton");
                browser.Wait();
                browser.Last("span").CheckIfInnerTextEquals("1");
            });
        }
    }
}