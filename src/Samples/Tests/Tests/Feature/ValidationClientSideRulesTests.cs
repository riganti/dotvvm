using System;
using System.Globalization;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class ValidationClientSideRulesTests : AppSeleniumTest
    {
        public ValidationClientSideRulesTests(ITestOutputHelper output) : base(output)
        {
        }

        (int requests, int postbacks) PostbacksCounts(IBrowserWrapper browser)
        {
            var requestCount = browser.Single("request-count", SelectByDataUi).GetInnerText();
            var postbackCount = browser.Single("postback-count", SelectByDataUi).GetInnerText();

            return (int.Parse(requestCount), int.Parse(postbackCount));
        }
        void ExpectNoErrors(IBrowserWrapper browser)
        {
            var count = PostbacksCounts(browser);
            browser.Single("submit-button", SelectByDataUi).Click();
            try
            {
                AssertUI.InnerTextEquals(browser.Single("postback-count", SelectByDataUi), (count.postbacks + 1).ToString());
                AssertUI.InnerTextEquals(browser.Single("request-count", SelectByDataUi), (count.requests + 1).ToString());
                AssertUI.InnerTextEquals(browser.Single("result", SelectByDataUi), "Valid");
                Assert.Empty(browser.FindElements("ul[data-ui=errors] > li"));
            }
            catch (Exception e)
            {
                var errors = browser.FindElements("ul[data-ui=errors] > li").Select(t => t.GetInnerText()).ToArray();
                if (errors.Length > 0)
                {
                    throw new Exception($"Validation failed with errors: {string.Join(", ", errors)}", e);
                }
                throw;
            }
        }

        void ExpectErrors(IBrowserWrapper browser, string[] expectedErrors)
        {
            var count = PostbacksCounts(browser);
            // client-side validation
            browser.Single("submit-button", SelectByDataUi).Click();
            AssertUI.InnerTextEquals(browser.Single("postback-count", SelectByDataUi), (count.postbacks + 1).ToString());
            var errors = browser.WaitFor(_ => {
                var c = browser.FindElements("ul[data-ui=errors] > li");
                Assert.NotEmpty(c);
                return c;
            }).Select(t => t.GetInnerText()).ToArray();
            AssertUI.InnerTextEquals(browser.Single("request-count", SelectByDataUi), count.requests.ToString(), failureMessage: $"Validation didn't run client-side (got errors: {string.Join(", ", errors)}.");
            AssertUI.InnerTextEquals(browser.Single("result", SelectByDataUi), "");
            Assert.Equal(expectedErrors.OrderBy(t => t).ToArray(), errors.OrderBy(t => t).ToArray());

            // server-side validation
            browser.Single("submit-button-serverside", SelectByDataUi).Click();
            AssertUI.InnerTextEquals(browser.Single("postback-count", SelectByDataUi), (count.postbacks + 2).ToString());
            AssertUI.InnerTextEquals(browser.Single("request-count", SelectByDataUi), (count.requests + 1).ToString());
            AssertUI.InnerTextEquals(browser.Single("result", SelectByDataUi), "");
            errors = browser.WaitFor(_ => {
                var c = browser.FindElements("ul[data-ui=errors] > li");
                Assert.NotEmpty(c);
                return c;
            }).Select(t => t.GetInnerText()).ToArray();
            Assert.Equal(expectedErrors.OrderBy(t => t).ToArray(), errors.OrderBy(t => t).ToArray());
        }

        void SetValue(IBrowserWrapper browser, string property, string value)
        {
            if (value == null)
                browser.Single($"setnull-{property}", SelectByDataUi).Click();
            else
                browser.Single($"textbox-{property}", SelectByDataUi).Clear().SendKeys(value);
        }

        [Theory]
        [InlineData("10", null)]
        [InlineData("20", null)]
        [InlineData("", null)]
        [InlineData(null, null)]
        [InlineData("-1", "The field RangeInt32 must be between 10 and 20.")]
        [InlineData("0", "The field RangeInt32 must be between 10 and 20.")]
        public void Feature_Validation_ClientSideRules_RangeInt32(string value, string error)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ClientSideRules);

                SetValue(browser, "RangeInt32", value);
                if (error == null)
                    ExpectNoErrors(browser);
                else
                    ExpectErrors(browser, new[] { error });
            });
        }

        [Theory]
        [InlineData("12.345678901", null)]
        [InlineData("Infinity", null)]
        [InlineData("3e300", null)]
        [InlineData(null, null)]
        [InlineData("12.345678900", "The field RangeFloat64 must be between 12.345678901 and ∞.")]
        // [InlineData("-Infinity", "The field RangeFloat64 must be between 12.345678901 and ∞.")]
        public void Feature_Validation_ClientSideRules_RangeFloat64(string value, string error)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ClientSideRules);

                SetValue(browser, "RangeFloat64", value);
                if (error == null)
                    ExpectNoErrors(browser);
                else
                    ExpectErrors(browser, new[] { error });
            });
        }

        [Theory]
        [InlineData("2015-01-01", null)]
        [InlineData("2015-12-31", null)]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("2024-01-01", "RangeDate must be between 2015-01-01T00:00:00 and 2015-12-31T23:59:59.")]
        public void Feature_Validation_ClientSideRules_RangeDate(string value, string error)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ClientSideRules);

                SetValue(browser, "RangeDate", value);
                if (error == null)
                    ExpectNoErrors(browser);
                else
                    ExpectErrors(browser, new[] { error });
            });
        }

        [Theory]
        [InlineData("12", null)]
        [InlineData(".", null)]
        [InlineData("", "The RequiredString field is required.")]
        [InlineData("   ", "The RequiredString field is required.")]
        [InlineData(null, "The RequiredString field is required.")]
        public void Feature_Validation_ClientSideRules_RequiredString(string value, string error)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ClientSideRules);

                SetValue(browser, "RequiredString", value);
                if (error == null)
                    ExpectNoErrors(browser);
                else
                    ExpectErrors(browser, new[] { error });
            });
        }

        [Theory]
        [InlineData("12", null)]
        [InlineData(".", null)]
        [InlineData("", null)]
        [InlineData("   ", null)]
        [InlineData(null, "The NotNullString field is required.")]
        public void Feature_Validation_ClientSideRules_NotNullString(string value, string error)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ClientSideRules);

                SetValue(browser, "NotNullString", value);
                if (error == null)
                    ExpectNoErrors(browser);
                else
                    ExpectErrors(browser, new[] { error });
            });
        }

        [Theory]
        [InlineData("a@b.c", null)]
        [InlineData(null, null)]
        [InlineData("@handle", "The EmailString field is not a valid e-mail address.")]
        [InlineData("incomplete@", "The EmailString field is not a valid e-mail address.")]
        [InlineData("", "The EmailString field is not a valid e-mail address.")]
        public void Feature_Validation_ClientSideRules_EmailString(string value, string error)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ClientSideRules);

                SetValue(browser, "EmailString", value);
                if (error == null)
                    ExpectNoErrors(browser);
                else
                    ExpectErrors(browser, new[] { error });
            });
        }

    }
}
