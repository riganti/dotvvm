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
    public class DateTimeSerializationTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_DateTimeSerialization()
        {
            var culture = new CultureInfo("cs-CZ");

            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Feature_DateTimeSerialization_DateTimeSerialization);

                // verify the first date
                browser.ElementAt("input[type=text]", 0).Clear().SendKeys("18.2.1988");
                browser.ElementAt("input[type=button]", 1).Click().Wait();
                
                //TODO: check with some other function
                Assert.AreEqual(new DateTime(1988, 2, 18), DateTime.Parse(browser.FindElements("span")[0].GetText()));

                browser.ElementAt("input[type=text]", 0).Clear().SendKeys("test");
                browser.ElementAt("input[type=button]", 1).Click().Wait();

                Assert.AreEqual(DateTime.MinValue, DateTime.Parse(browser.FindElements("span")[0].GetText()));

                // verify the second date
                browser.ElementAt("input[type=text]", 1).Clear().SendKeys("2011-03-19 16:48:17");
                browser.ElementAt("input[type=button]", 3).Click().Wait();

                Assert.AreEqual(new DateTime(2011, 3, 19, 16, 48, 0),
                    DateTime.Parse(browser.FindElements("span")[1].GetText()));

                browser.ElementAt("input[type=text]", 1).Clear().SendKeys("test");
                browser.ElementAt("input[type=button]", 3).Click().Wait();

                browser.ElementAt("span", 1).CheckIfInnerTextEquals("null");

                // try to set dates from server
                browser.ElementAt("input[type=button]", 0).Click().Wait();
                browser.ElementAt("input[type=button]", 2).Click().Wait();

                Assert.IsTrue((DateTime.Now - DateTime.Parse(browser.FindElements("input[type=text]")[0].GetAttribute("value"), culture)).TotalHours < 24); // there is no time in the field
                Assert.IsTrue((DateTime.Now - DateTime.Parse(browser.FindElements("input[type=text]")[1].GetAttribute("value"), culture)).TotalMinutes < 1); // the minutes can differ slightly
            });
        }
    }
}