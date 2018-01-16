using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DotVVM.Samples.Tests.New.Feature
{
    public class PostbackConcurrencyTests : AppSeleniumTest
    {
        public PostbackConcurrencyTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_PostbackConcurrency_UpdateProgressControl()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_NoneMode);

                // test update progress control
                AssertUI.IsNotDisplayed(browser, "div[data-ui=update-progress]");
                browser.Single("input[data-ui=long-action-button]").Click();
                AssertUI.IsDisplayed(browser, "div[data-ui=update-progress]");
                browser.Wait(3000);
                AssertUI.IsNotDisplayed(browser, "div[data-ui=update-progress]");
            });
        }

        [Fact]
        public void Feature_PostbackConcurrency_NoneMode()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_NoneMode);

                // try the long action interrupted by the short one
                browser.Single("input[data-ui=long-action-button]").Click();
                browser.Wait(1000);
                browser.Single("input[data-ui=short-action-button]").Click();

                var postbackIndexSpan = browser.Single("span[data-ui=postback-index]");
                var lastActionSpan = browser.Single("span[data-ui=last-action]");

                // the postback index should be 1 now (because of short action)
                AssertUI.InnerTextEquals(postbackIndexSpan, "1");
                AssertUI.InnerTextEquals(lastActionSpan, "short");

                // the result of the long action should be canceled, the counter shouldn't increase
                browser.Wait(6000);
                AssertUI.InnerTextEquals(postbackIndexSpan, "1");
                AssertUI.InnerTextEquals(lastActionSpan, "short");
            });
        }

        [Fact]
        public void Feature_PostbackConcurrency_QueueMode()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_QueueMode);

                // try the long action than queue another long action and short action
                browser.Single("input[data-ui=long-action-button]").Click();
                browser.Wait(500);
                browser.Single("input[data-ui=long-action-button]").Click();
                browser.Wait(500);
                browser.Single("input[data-ui=short-action-button]").Click();

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

        [Fact]
        public void Feature_PostbackConcurrency_DenyMode()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackConcurrency_DenyMode);

                // try the long action than queue the short action which should fail
                browser.Single("input[data-ui=long-action-button]").Click();
                browser.Wait(500);
                browser.Single("input[data-ui=short-action-button]").Click();

                var postbackIndexSpan = browser.Single("span[data-ui=postback-index]");
                var lastActionSpan = browser.Single("span[data-ui=last-action]");

                // the postback index should be 0 now (because of no postback finished yet)
                AssertUI.InnerTextEquals(postbackIndexSpan, "0");
                AssertUI.InnerTextEquals(lastActionSpan, string.Empty);

                browser.Wait(4000);

                // the long action should be finished and the short action should be interrupted with no effect
                AssertUI.InnerTextEquals(postbackIndexSpan, "1");
                AssertUI.InnerTextEquals(lastActionSpan, "long");
            });
        }
    }
}
