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
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_DateTimeSerialization_DateTimeSerialization);

                // verify the first date
                browser.ElementAt("input[type=text]", 0).Clear().SendKeys("18.2.1988");
                browser.ElementAt("input[type=button]", 1).Click();

                browser.ElementAt("span", 0).CheckIfInnerText(s => DateTime.Parse(s).Equals(new DateTime(1988, 2, 18)));
                browser.ElementAt("input[type=text]", 0).Clear().SendKeys("test");
                browser.ElementAt("input[type=button]", 1).Click();

                browser.ElementAt("span", 0).CheckIfInnerText(s => DateTime.Parse(s).Equals(DateTime.MinValue));
                
                // verify the second date
                browser.ElementAt("input[type=text]", 1).Clear().SendKeys("2011-03-19 16:48:17");
                browser.ElementAt("input[type=button]", 3).Click();

                browser.ElementAt("span", 1).CheckIfInnerText(s => DateTime.Parse(s).Equals(new DateTime(2011, 3, 19, 16, 48, 0)));
                
                browser.ElementAt("input[type=text]", 1).Clear().SendKeys("test");
                browser.ElementAt("input[type=button]", 3).Click();

                browser.ElementAt("span", 1).CheckIfInnerTextEquals("null");

                // try to set dates from server
                browser.ElementAt("input[type=button]", 0).Click();
                browser.ElementAt("input[type=button]", 2).Click();

                // there is no time in the field
                browser.ElementAt("input[type=text]", 0)
                    .CheckAttribute("value", s => (DateTime.Now - DateTime.Parse(s, culture)).TotalHours < 24);

                // the minutes can differ slightly
                browser.ElementAt("input[type=text]", 1)
                    .CheckAttribute("value", s => (DateTime.Now - DateTime.Parse(s, culture)).TotalMinutes < 1);
            });
        }
    }
}