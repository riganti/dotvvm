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

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class PostBackUpdateTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_PostBackUpdate()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("FeatureSamples/PostBackUpdate/PostBackUpdate");

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
    }
}