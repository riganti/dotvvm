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

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class PropertyUpdateTests : SeleniumTestBase
    {
        [TestMethod]
        public void PropertyUpdateTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("ControlSamples/PropertyUpdate");

                // enter number of lines and click the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "15");
                browser.Click("input[type=button]");
                browser.Wait();

                browser.First("span")
                    .CheckIfInnerText(s => Regex.Matches(s, "Lorem ipsum").Count == 15,
                        "Output doesn't contain expected number of values");
                //Assert.AreEqual(14, browser.FindElements("br").Count);

                // change number of lines and click the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "5");
                browser.Click("input[type=button]");
                browser.Wait();

                browser.First("span")
                    .CheckIfInnerText(s => Regex.Matches(s, "Lorem ipsum").Count == 5,
                        "Output doesn't contain expected number of values");
                //Assert.AreEqual(4, browser.FindElements("br").Count);
            });
        }
    }
}