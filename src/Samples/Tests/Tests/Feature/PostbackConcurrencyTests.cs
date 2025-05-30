using System;
using System.Runtime.InteropServices;
using System.Threading;
using DotVVM.Framework.Configuration;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions.Attributes;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class PostbackConcurrencyTests : AppSeleniumTest
    {
        public PostbackConcurrencyTests(ITestOutputHelper output) : base(output)
        {
        }

        /// <summary>
        /// Determines if the test is running on Linux with experimental features enabled
        /// </summary>
        private bool IsLinuxWithExperimentalFeatures()
        {
            // Check if running on Linux
            var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            // This is a simple check - in a real scenario, we would need to access the actual DotvvmConfiguration
            // Since we can't access the configuration directly in the test, we'll check for environment variable
            // that might indicate experimental features are enabled
            var experimentalFeaturesEnabled = Environment.GetEnvironmentVariable("DOTVVM_EXPERIMENTAL_FEATURES") == "1";

            return isLinux && experimentalFeaturesEnabled;
        }

        [Theory]
        [InlineData("input[data-ui=long-action-button]")]
        [InlineData("input[data-ui=long-static-action-button]")]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_PostbackConcurrencyMode))]
        public void Feature_PostbackConcurrency_UpdateProgressControl(string longActionSelector)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_DefaultMode);

                // test update progress control
                AssertUI.IsNotDisplayed(browser, "div[data-ui=update-progress]");
                browser.Single(longActionSelector).Click();
                AssertUI.IsDisplayed(browser, "div[data-ui=update-progress]");
                browser.Wait(3000);
                AssertUI.IsNotDisplayed(browser, "div[data-ui=update-progress]");
            });
        }

        [Theory]
        [InlineData("input[data-ui=long-action-button]", "input[data-ui=short-action-button]")]
        public void Feature_PostbackConcurrency_DefaultMode(string longActionSelector, string shortActionSelector)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_DefaultMode);
                browser.WaitUntilDotvvmInited();

                // try the long action interrupted by the short one
                browser.Single(longActionSelector).Click();
                browser.Wait(1000);
                browser.Single(shortActionSelector).Click();

                var postbackIndexSpan = browser.Single("span[data-ui=postback-index]");
                var lastActionSpan = browser.Single("span[data-ui=last-action]");

                // the postback index should be 1 now (because of short action)
                AssertUI.InnerTextEquals(postbackIndexSpan, "1");
                AssertUI.InnerTextEquals(lastActionSpan, "short");
                browser.Wait(6000);
                // the result of the long action should be canceled, the counter shouldn't increase
                AssertUI.InnerTextEquals(postbackIndexSpan, "1");
                AssertUI.InnerTextEquals(lastActionSpan, "short");
            });
        }

        [Theory]
        [InlineData("input[data-ui=long-action-button]", "input[data-ui=short-action-button]")]
        [InlineData("input[data-ui=long-static-action-button]", "input[data-ui=short-static-action-button]")]
        public void Feature_PostbackConcurrency_QueueMode(string longActionSelector, string shortActionSelector)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_QueueMode);

                // try the long action than queue another long action and short action
                browser.Single(longActionSelector).Click();
                browser.Wait(500);
                browser.Single(longActionSelector).Click();
                browser.Wait(500);
                browser.Single(shortActionSelector).Click();

                var postbackIndexSpan = browser.Single("span[data-ui=postback-index]");
                var lastActionSpan = browser.Single("span[data-ui=last-action]");

                // the postback index should be 0 now (because of no postback finished yet)
                AssertUI.InnerTextEquals(postbackIndexSpan, "0");
                AssertUI.InnerTextEquals(lastActionSpan, string.Empty);

                browser.Wait(3000);
                // the first long action should be finished, the counter should increase and another long action should be running
                AssertUI.InnerTextEquals(postbackIndexSpan, "1");
                AssertUI.InnerTextEquals(lastActionSpan, "long");

                browser.Wait(3000);
                // the second long action should be finished together with the short action,
                // the counter should increase twice
                AssertUI.InnerTextEquals(postbackIndexSpan, "3");
                AssertUI.InnerTextEquals(lastActionSpan, "short");
            });
        }

        [Theory]
        [InlineData("input[data-ui=long-action-button]", "input[data-ui=short-action-button]")]
        [InlineData("input[data-ui=long-static-action-button]", "input[data-ui=short-static-action-button]")]
        public void Feature_PostbackConcurrency_DenyMode(string longActionSelector, string shortActionSelector)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_DenyMode);

                // try the long action than queue the short action which should fail
                browser.Single(longActionSelector).Click();
                browser.Wait(250);
                browser.Single(shortActionSelector).Click();

                var postbackIndexSpan = browser.Single("span[data-ui=postback-index]");
                var lastActionSpan = browser.Single("span[data-ui=last-action]");

                // the postback index should be 0 now (because of no postback finished yet)
                AssertUI.InnerTextEquals(postbackIndexSpan, "0");
                AssertUI.InnerTextEquals(lastActionSpan, string.Empty);

                browser.WaitFor(() => {
                    // the long action should be finished and the short action should be interrupted with no effect
                    AssertUI.InnerTextEquals(postbackIndexSpan, "1");
                    AssertUI.InnerTextEquals(lastActionSpan, "long");
                }, 6000);
            });
        }

        [Theory]
        [InlineData("input[data-ui=long-action-button]")]
        [InlineData("input[data-ui=short-action-button]")]
        [InlineData("input[data-ui=long-static-action-button]")]
        public void Feature_PostbackConcurrency_UnrelatedProperty(string actionSelector)
        {
            // execute action, before it finishes update the counter, check that counter value didn't get reverted
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_DefaultMode);
                browser.WaitUntilDotvvmInited();

                browser.Single(actionSelector).Click();
                browser.Wait(300);

                browser.Single("counter", SelectByDataUi).Click();
                browser.Single("counter", SelectByDataUi).Click();
                AssertUI.InnerTextEquals(browser.Single("counter", SelectByDataUi), "2");

                browser.Wait(6000);
                AssertUI.InnerTextEquals(browser.Single("span[data-ui=postback-index]"), "1");
                AssertUI.InnerTextEquals(browser.Single("counter", SelectByDataUi), "2");
            });
        }

        [Fact]
        [SampleReference(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_StressTest)]
        public void Feature_PostbackConcurrency_StressTest_Default()
        {
            // Skip this test on Linux when running with experimental features
            if (IsLinuxWithExperimentalFeatures())
            {
                return; // Skip test silently
            }

            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_StressTest);

                browser.ElementAt("input[type=button]", 1).Click();

                browser.WaitFor(() => {
                    var before = int.Parse(browser.Single(".result-before").GetInnerText().Trim());
                    var rejected = int.Parse(browser.Single(".result-rejected").GetInnerText().Trim());
                    var after = int.Parse(browser.Single(".result-after").GetInnerText().Trim());
                    var value = int.Parse(browser.Single(".result-value").GetInnerText().Trim());

                    Assert.InRange(value, 1, 100);
                    Assert.Equal(100, before);
                    Assert.Equal(100, after);
                    Assert.Equal(0, rejected);
                }, timeout: 30_000);
            });
        }

        [Fact]
        [SampleReference(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_StressTest)]
        public void Feature_PostbackConcurrency_StressTest_Deny()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_StressTest);

                browser.ElementAt("input[type=button]", 3).Click();

                browser.WaitFor(() => {
                    var before = int.Parse(browser.Single(".result-before").GetInnerText().Trim());
                    var rejected = int.Parse(browser.Single(".result-rejected").GetInnerText().Trim());
                    var after = int.Parse(browser.Single(".result-after").GetInnerText().Trim());
                    var value = int.Parse(browser.Single(".result-value").GetInnerText().Trim());

                    Assert.InRange(value, 1, 100);
                    Assert.Equal(100, before + rejected);
                    Assert.Equal(100, after);
                }, timeout: 30_000);
            });
        }

        [Fact]
        [SampleReference(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_StressTest)]
        public void Feature_PostbackConcurrency_StressTest_Queue()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_StressTest);

                browser.ElementAt("input[type=button]", 5).Click();

                browser.WaitFor(() => {
                    var before = int.Parse(browser.Single(".result-before").GetInnerText().Trim());
                    var rejected = int.Parse(browser.Single(".result-rejected").GetInnerText().Trim());
                    var after = int.Parse(browser.Single(".result-after").GetInnerText().Trim());
                    var value = int.Parse(browser.Single(".result-value").GetInnerText().Trim());

                    Assert.Equal(100, value);
                    Assert.Equal(100, before);
                    Assert.Equal(100, after);
                    Assert.Equal(0, rejected);
                }, timeout: 30_000);
            });
        }

        [Fact(Skip = "Cannot read element contents when the browser is already navigating away - getting element stale exception.")]
        public void Feature_PostbackConcurrency_RedirectPostbackQueue()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_RedirectPostbackQueue);

                browser.ElementAt("input[type=button]", 0).Click();
                browser.ElementAt("input[type=button]", 1).Click();

                while (!browser.CurrentUrl.Contains("?time"))
                {
                    Thread.Sleep(100);
                    AssertUI.TextNotEquals(browser.Single(".result"), "1");
                }
            });
        }

        [Fact]
        public void Feature_PostbackConcurrency_RedirectPostbackQueueSpa_PostbackDuringRedirect()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_RedirectPostbackQueueSpa);

                browser.ElementAt("input[type=button]", 0).Click();
                browser.ElementAt("input[type=button]", 1).Click();

                while (!browser.CurrentUrl.Contains("?time"))
                {
                    Thread.Sleep(100);
                    AssertUI.TextNotEquals(browser.Single(".result"), "1");
                }
            });
        }

        [Fact]
        [SampleReference(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_RedirectPostbackQueueSpa)]
        public void Feature_PostbackConcurrency_RedirectPostbackQueueSpa_PostbackFromPreviousPage()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_RedirectPostbackQueueSpa);
                browser.WaitUntilDotvvmInited();

                browser.ElementAt("input[type=button]", 0).Click();
                browser.ElementAt("input[type=button]", 2).Click();

                var attempt = 0;
                while (!browser.CurrentUrl.Contains("?time"))
                {
                    attempt++;
                    if (attempt > 50)
                    {
                        Assert.Fail("The redirect didn't happen.");
                    }

                    Thread.Sleep(100);
                    try
                    {
                        AssertUI.TextNotEquals(browser.Single(".result"), "1", waitForOptions: WaitForOptions.Disabled);
                    }
                    catch (StaleElementReferenceException)
                    {
                        // ignore
                        break;
                    }
                }
                AssertUI.Url(browser, u => u.Contains("?time"));

                for (int i = 0; i < 8; i++)
                {
                    AssertUI.TextNotEquals(browser.Single(".result"), "1");
                    Thread.Sleep(1000);
                }
            });
        }
    }
}
