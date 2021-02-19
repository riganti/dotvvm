using System.Threading;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
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

        [Theory]
        [InlineData("input[data-ui=long-action-button]")]
        [InlineData("input[data-ui=long-static-action-button]")]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_PostbackConcurrencyMode))]
        [SkipBrowser("ie:fast", reason: "This scenario works in IE but it's hard to time it properly because click in IE last 500 ms avg")]
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
        [SkipBrowser("ie:fast", reason: "This scenario works in IE but it's hard to time it properly because click in IE last 500 ms avg")]
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
                browser.WaitFor(()=> {
                    AssertUI.InnerTextEquals(postbackIndexSpan, "1");
                    AssertUI.InnerTextEquals(lastActionSpan, "short");
                },3000);
            });
        }

        [Theory]
        [InlineData("input[data-ui=long-action-button]", "input[data-ui=short-action-button]")]
        [InlineData("input[data-ui=long-static-action-button]", "input[data-ui=short-static-action-button]")]
        [SkipBrowser("ie:fast", reason: "This scenario works in IE but it's hard to time it properly because click in IE last 500 ms avg")]
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
        [SkipBrowser("ie:fast", reason: "This scenario works in IE but it's hard to time it properly because click in IE last 500 ms avg")]
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

        [Fact]
        [SampleReference(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_StressTest)]
        public void Feature_PostbackConcurrency_StressTest_Default()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_StressTest);

                browser.ElementAt("input[type=button]", 1).Click();

                Thread.Sleep(10000);
                var before = int.Parse(browser.Single(".result-before").GetInnerText().Trim());
                var rejected = int.Parse(browser.Single(".result-rejected").GetInnerText().Trim());
                var after = int.Parse(browser.Single(".result-after").GetInnerText().Trim());
                var value = int.Parse(browser.Single(".result-value").GetInnerText().Trim());

                Assert.True(0 < value && value <= 100);
                Assert.Equal(100, before);
                Assert.Equal(100, after);
                Assert.Equal(0, rejected);
            });
        }

        [Fact]
        [SampleReference(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_StressTest)]
        public void Feature_PostbackConcurrency_StressTest_Deny()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_StressTest);

                browser.ElementAt("input[type=button]", 3).Click();

                Thread.Sleep(10000);
                var before = int.Parse(browser.Single(".result-before").GetInnerText().Trim());
                var rejected = int.Parse(browser.Single(".result-rejected").GetInnerText().Trim());
                var after = int.Parse(browser.Single(".result-after").GetInnerText().Trim());
                var value = int.Parse(browser.Single(".result-value").GetInnerText().Trim());

                Assert.True(0 < value && value <= 100);
                Assert.Equal(100, before + rejected);
                Assert.Equal(100, after);
            });
        }

        [Fact]
        [SampleReference(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_StressTest)]
        public void Feature_PostbackConcurrency_StressTest_Queue()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_StressTest);

                browser.ElementAt("input[type=button]", 5).Click();

                Thread.Sleep(10000);
                var before = int.Parse(browser.Single(".result-before").GetInnerText().Trim());
                var rejected = int.Parse(browser.Single(".result-rejected").GetInnerText().Trim());
                var after = int.Parse(browser.Single(".result-after").GetInnerText().Trim());
                var value = int.Parse(browser.Single(".result-value").GetInnerText().Trim());

                Assert.Equal(100, value);
                Assert.Equal(100, before);
                Assert.Equal(100, after);
                Assert.Equal(0, rejected);
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

                browser.ElementAt("input[type=button]", 0).Click();
                browser.ElementAt("input[type=button]", 2).Click();

                while (!browser.CurrentUrl.Contains("?time"))
                {
                    Thread.Sleep(100);
                    AssertUI.TextNotEquals(browser.Single(".result"), "1");
                }

                for (int i = 0; i < 8; i++)
                {
                    AssertUI.TextNotEquals(browser.Single(".result"), "1");
                    Thread.Sleep(1000);
                }
            });
        }
    }
}
