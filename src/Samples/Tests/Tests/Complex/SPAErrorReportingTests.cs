using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions.Attributes;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    public class SPAErrorReportingTests : AppSeleniumTest
    {
        public SPAErrorReportingTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPAErrorReporting_default))]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPAErrorReporting_test))]
        [SkipBrowser("firefox:dev", "Cannot simulate offline mode.")]
        [SkipBrowser("firefox:fast", "Cannot simulate offline mode.")]
        [SkipBrowser("firefox:fast", "Cannot simulate offline mode.")]
        [Trait("Category", "dev-only")] // error page
        public void Complex_SPAErrorReporting_NavigationAndPostbacks()
        {
            RunInAllBrowsers(browser => {

                void SetOfflineMode(bool offline)
                {
                    ((ChromeDriver)browser.Driver).NetworkConditions = new ChromiumNetworkConditions() {
                        IsOffline = offline,
                        Latency = TimeSpan.FromMilliseconds(5),
                        DownloadThroughput = 500 * 1024,
                        UploadThroughput = 500 * 1024
                    };
                }

                try
                {
                    browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_SPAErrorReporting_default);

                    // go to Test page and verify the success
                    browser.ElementAt("a", 1).Click();
                    browser.WaitForPostback();
                    AssertUI.TextEquals(browser.Single("h2"), "Test");

                    SetOfflineMode(true);

                    // try to submit command in offline mode (we don't have CSRF token in Lazy CSRF mode yet, so we should fail in fetchCsrfToken)
                    browser.Single("input[type=text]").SendKeys("aaa");
                    browser.ElementAt("input[type=button]", 0).Click();
                    browser.Single("#debugWindow button").Click();
                    AssertUI.TextEquals(browser.Single("#numberOfErrors"), "1");

                    // try to submit static command in offline mode
                    browser.Single("input[type=text]").SendKeys("aaa");
                    browser.ElementAt("input[type=button]", 1).Click();
                    browser.Single("#debugWindow button").Click();
                    AssertUI.TextEquals(browser.Single("#numberOfErrors"), "2");

                    // try to go back in offline mode
                    browser.ElementAt("a", 0).Click();
                    AssertUI.TextEquals(browser.Single("h2"), "Test");
                    browser.Single("#debugWindow button").Click();
                    AssertUI.TextEquals(browser.Single("#numberOfErrors"), "3");

                    SetOfflineMode(false);

                    // go back to online mode and retry (we should now obtain the CSRF token in lazy CSRF mode)
                    browser.ElementAt("input[type=button]", 0).Click();
                    AssertUI.TextEquals(browser.Single("#numberOfErrors"), "3");
                    AssertUI.TextEquals(browser.Single("*[data-ui=sample-text]"), "Sample Text");

                    browser.ElementAt("input[type=button]", 1).Click();
                    AssertUI.TextEquals(browser.Single("#numberOfErrors"), "3");
                    AssertUI.TextEquals(browser.Single("*[data-ui=sample-text]"), "Sample Static Text");

                    browser.ElementAt("a", 0).Click();
                    AssertUI.TextEquals(browser.Single("h2"), "Default");
                    AssertUI.TextEquals(browser.Single("#numberOfErrors"), "3");

                    browser.ElementAt("a", 1).Click();
                    AssertUI.TextEquals(browser.Single("h2"), "Test");
                    AssertUI.TextEquals(browser.Single("#numberOfErrors"), "3");

                    SetOfflineMode(true);

                    // try to submit command in offline mode again (now we have valid CSRF token so we should fail on the postback itself)
                    browser.Single("input[type=text]").SendKeys("aaa");
                    browser.ElementAt("input[type=button]", 0).Click();
                    browser.Single("#debugWindow button").Click();
                    AssertUI.TextEquals(browser.Single("#numberOfErrors"), "4");

                    // try to submit static command in offline mode
                    browser.Single("input[type=text]").SendKeys("aaa");
                    browser.ElementAt("input[type=button]", 1).Click();
                    browser.Single("#debugWindow button").Click();
                    AssertUI.TextEquals(browser.Single("#numberOfErrors"), "5");
                }
                finally
                {
                    SetOfflineMode(false);
                }
            });
        }
    }
}
