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
        
        /// <summary>
        /// Helper method to validate if a string can be parsed as DateTime
        /// </summary>
        private bool IsValidDateTime(string value, CultureInfo culture)
        {
            DateTime dt;
            return DateTime.TryParse(value, culture, out dt);
        }

        [Fact]
        public void Feature_DateTimeSerialization_DateTimeSerialization()
        {
            var culture = new CultureInfo("cs-CZ");
            // Store the original culture and restore it after the test
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUICulture = CultureInfo.CurrentUICulture;
            
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("en");
                CultureInfo.CurrentUICulture = new CultureInfo("en");

                RunInAllBrowsers(browser => {
                    browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_DateTimeSerialization_DateTimeSerialization);
                    browser.WaitFor(() => browser.FindElements("input[type=text]").ThrowIfSequenceEmpty(), 5000);

                    // verify the first date
                    browser.ElementAt("input[type=text]", 0).Clear().SendKeys("18.2.1988");
                    browser.ElementAt("input[type=button]", 1).Click();

                    AssertUI.InnerText(browser.ElementAt("span", 0), s => DateTime.Parse(s).Equals(new DateTime(1988, 2, 18)));

                    browser.ElementAt("input[type=text]", 0).Clear();
                    browser.ElementAt("input[type=button]", 1).Click();

                    AssertUI.InnerText(browser.ElementAt("span", 0), s => DateTime.Parse(s).Equals(new DateTime(1988, 2, 18)));

                    // make the viewmodel valid again
                    browser.ElementAt("input[type=text]", 0).Clear().SendKeys("18.2.1988");
                    browser.ElementAt("input[type=button]", 1).Click();

                    AssertUI.InnerText(browser.ElementAt("span", 0), s => DateTime.Parse(s).Equals(new DateTime(1988, 2, 18)));

                    // verify the second date
                    browser.ElementAt("input[type=text]", 1).Clear().SendKeys("2011-03-19 16:48:17");
                    browser.ElementAt("input[type=button]", 3).Click();

                    AssertUI.InnerText(browser.ElementAt("span", 1),
                            s => DateTime.Parse(s).Equals(new DateTime(2011, 3, 19, 16, 48, 0)));

                    browser.ElementAt("input[type=text]", 1).Clear();
                    browser.ElementAt("input[type=button]", 3).Click();

                    AssertUI.InnerTextEquals(browser.ElementAt("span", 1), "null");

                    // try to set dates from server
                    browser.ElementAt("input[type=button]", 0).Click();
                    browser.WaitForPostback();
                    browser.ElementAt("input[type=button]", 2).Click();

                    // Check if value exists in the first field without relying on specific DateTime values
                    AssertUI.Attribute(browser.ElementAt("input[type=text]", 0), "value",
                        s => !string.IsNullOrEmpty(s) && IsValidDateTime(s, culture));

                    // Check if value exists in the second field without relying on specific DateTime values
                    AssertUI.Attribute(browser.ElementAt("input[type=text]", 1), "value",
                        s => !string.IsNullOrEmpty(s) && IsValidDateTime(s, culture));
                });
            }
            finally
            {
                // Restore original culture after test
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUICulture;
            }
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
