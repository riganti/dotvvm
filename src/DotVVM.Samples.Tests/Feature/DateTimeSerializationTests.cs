using System;
using System.Globalization;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class DateTimeSerializationTests : AppSeleniumTest
    {
        public DateTimeSerializationTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_DateTimeSerialization_DateTimeSerialization()
        {
            var culture = new CultureInfo("cs-CZ");
            CultureInfo.CurrentCulture = new CultureInfo("en");

            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_DateTimeSerialization_DateTimeSerialization);
                browser.WaitFor(() => browser.FindElements("input[type=text]").ThrowIfSequenceEmpty(), 5000);

                // verify the first date
                browser.ElementAt("input[type=text]", 0).Clear().SendKeys("18.2.1988");
                browser.ElementAt("input[type=button]", 1).Click();

                browser.WaitFor(() => {
                    AssertUI.InnerText(browser.ElementAt("span", 0), s => DateTime.Parse(s).Equals(new DateTime(1988, 2, 18)));
                }, 5000);

                browser.ElementAt("input[type=text]", 0).Clear();
                browser.ElementAt("input[type=button]", 1).Click();

                browser.WaitFor(() => {
                    // the value is invalid so the viewmodel stays as is
                    AssertUI.InnerText(browser.ElementAt("span", 0), s => DateTime.Parse(s).Equals(new DateTime(1988, 2, 18)));
                }, 5000);

                // make the viewmodel valid again
                browser.ElementAt("input[type=text]", 0).Clear().SendKeys("18.2.1988");
                browser.ElementAt("input[type=button]", 1).Click();

                browser.WaitFor(() => {
                    AssertUI.InnerText(browser.ElementAt("span", 0), s => DateTime.Parse(s).Equals(new DateTime(1988, 2, 18)));
                }, 5000);

                // verify the second date
                browser.ElementAt("input[type=text]", 1).Clear().SendKeys("2011-03-19 16:48:17");
                browser.ElementAt("input[type=button]", 3).Click();

                browser.WaitFor(() => {
                    AssertUI.InnerText(browser.ElementAt("span", 1),
                        s => DateTime.Parse(s).Equals(new DateTime(2011, 3, 19, 16, 48, 0)));
                }, 5000);

                browser.ElementAt("input[type=text]", 1).Clear();
                browser.ElementAt("input[type=button]", 3).Click();

                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(browser.ElementAt("span", 1), "null");
                }, 5000);

                // try to set dates from server
                browser.ElementAt("input[type=button]", 0).Click();
                browser.WaitForPostback();
                browser.ElementAt("input[type=button]", 2).Click();

                browser.WaitFor(() => {
                    // there is no time in the field
                    AssertUI.Attribute(browser.ElementAt("input[type=text]", 0), "value",
                        s => (DateTime.Now - DateTime.Parse(s, culture)).TotalHours < 24);

                    // the minutes can differ slightly
                    AssertUI.Attribute(browser.ElementAt("input[type=text]", 1), "value",
                        s => (DateTime.Now - DateTime.Parse(s, culture)).TotalMinutes < 1);
                }, 5000);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_DateTimeSerialization_DateTimeSerialization))]
        public void Feature_DateTimeSerialization_StaticDateTime()
        {
            var culture = new CultureInfo("cs-CZ");
            CultureInfo.CurrentCulture = new CultureInfo("en");

            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_DateTimeSerialization_DateTimeSerialization);

                AssertUI.Attribute(browser.Single("input[data-ui='static-date']"), "value", s => string.IsNullOrEmpty(s));

                browser.Single("input[data-ui='set-static-date-button']").Click();

                AssertUI.Attribute(browser.Single("input[data-ui='static-date']"), "value",
                    s => DateTime.Parse(s, culture) == new DateTime(2000, 1, 1));
            });
        }
    }
}
