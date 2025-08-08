using System;
using System.Linq;
using System.Threading;
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
    public class RedirectTests : AppSeleniumTest
    {
        public RedirectTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_Redirect_Redirect()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Redirect_Redirect);

                var currentUrl = new Uri(browser.CurrentUrl);
                Assert.Matches(@"time=\d+", currentUrl.Query);

                browser.First("[data-ui=object-redirect-button]").Click();
                browser.WaitForPostback();
                currentUrl = new Uri(browser.CurrentUrl);
                Assert.Matches(@"^\?(param=temp1&time=\d+|time=\d+&param=temp1)$", currentUrl.Query);
                Assert.Equal("#test1", currentUrl.Fragment);

                browser.First("[data-ui=dictionary-redirect-button]").Click();
                browser.WaitForPostback();
                currentUrl = new Uri(browser.CurrentUrl);
                Assert.Matches(@"^\?(time=\d+&param=temp2|param=temp2&time=\d+)$", currentUrl.Query);
                Assert.Equal("#test2", currentUrl.Fragment);
            });
        }

        [Fact]
        public void Feature_Redirect_RedirectionHelpers()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Redirect_RedirectionHelpers);

                var currentUrl = new Uri(browser.CurrentUrl);
                Assert.Matches(SamplesRouteUrls.FeatureSamples_Redirect_RedirectionHelpers, currentUrl.LocalPath);

                browser.FindElements("a").First().Click();
                currentUrl = new Uri(browser.CurrentUrl);
                Assert.Matches($"https://www.dotvvm.com", currentUrl.AbsoluteUri);
                browser.NavigateBack();

                browser.FindElements("a").Skip(1).First().Click();
                currentUrl = new Uri(browser.CurrentUrl);
                Assert.Matches($"{SamplesRouteUrls.FeatureSamples_Redirect_RedirectionHelpers_PageC}/111", currentUrl.LocalPath);
                browser.NavigateBack();

                browser.FindElements("a").Skip(2).First().Click();
                currentUrl = new Uri(browser.CurrentUrl);
                Assert.Matches($"{SamplesRouteUrls.FeatureSamples_Redirect_RedirectionHelpers_PageE}/1221", currentUrl.LocalPath);

                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Redirect_RedirectionHelpers_PageB + "/1234?x=a");
                currentUrl = new Uri(browser.CurrentUrl);
                Assert.Matches($"{SamplesRouteUrls.FeatureSamples_Redirect_RedirectionHelpers_PageC}/1234\\?test=aaa", currentUrl.LocalPath + currentUrl.Query);

                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Redirect_RedirectionHelpers_PageD + "/1234?x=a");
                currentUrl = new Uri(browser.CurrentUrl);
                Assert.Matches($"{SamplesRouteUrls.FeatureSamples_Redirect_RedirectionHelpers_PageE}/1221\\?x=a", currentUrl.LocalPath + currentUrl.Query);
            });
        }

        bool TryClick(IElementWrapper element)
        {
            if (element is null) return false;
            try
            {
                element.Click();
                return true;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
            catch (UnknownErrorException)
            {
                return false;
            }
        }

        [Fact]
        public void Feature_Redirect_RedirectPostbackConcurrency()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Redirect_RedirectPostbackConcurrency);

                int globalCounter() => int.Parse(browser.First("counter", SelectByDataUi).GetText());

                var initialCounter = globalCounter();
                for (int i = 0; i < 20; i++)
                {
                    TryClick(browser.FirstOrDefault("inc-default", SelectByDataUi));
                    Thread.Sleep(1);
                }
                browser.WaitFor(() => Assert.Contains("empty=true", browser.CurrentUrl, StringComparison.OrdinalIgnoreCase), 7000, "Redirect did not happen");
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Redirect_RedirectPostbackConcurrency);

                // must increment at least 20 times, otherwise delays are too short
                Assert.Equal(globalCounter(), initialCounter + 20);

                initialCounter = globalCounter();
                var clickCount = 0;
                while (TryClick(browser.FirstOrDefault("inc-deny", SelectByDataUi)))
                {
                    clickCount++;
                    Thread.Sleep(1);
                }
                Assert.InRange(clickCount, 3, int.MaxValue);
                browser.WaitFor(() => Assert.Contains("empty=true", browser.CurrentUrl, StringComparison.OrdinalIgnoreCase), timeout: 500, "Redirect did not happen");

                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Redirect_RedirectPostbackConcurrency);
                Assert.Equal(globalCounter(), initialCounter + 1); // only one click was allowed

                initialCounter = globalCounter();
                clickCount = 0;
                while (TryClick(browser.FirstOrDefault("inc-queue", SelectByDataUi)))
                {
                    clickCount++;
                    Thread.Sleep(1);
                }

                Assert.InRange(clickCount, 3, int.MaxValue);
                browser.WaitFor(() => Assert.Contains("empty=true", browser.CurrentUrl, StringComparison.OrdinalIgnoreCase), timeout: 500, "Redirect did not happen");

                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Redirect_RedirectPostbackConcurrency);
                Assert.Equal(globalCounter(), initialCounter + 1); // only one click was allowed
            });
        }

        [Fact]
        public void Feature_Redirect_RedirectPostbackConcurrencyFileReturn()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Redirect_RedirectPostbackConcurrency);

                void increment(int timeout)
                {
                    browser.WaitFor(() => {
                        var original = int.Parse(browser.First("minicounter", SelectByDataUi).GetText());
                        browser.First("minicounter", SelectByDataUi).Click();
                        AssertUI.TextEquals(browser.First("minicounter", SelectByDataUi), (original + 1).ToString());
                    }, timeout, "Could not increment minicounter in given timeout (postback queue is blocked)");
                }

                increment(3000);

                browser.First("file-std", SelectByDataUi).Click();
                increment(3000);

                browser.First("file-custom", SelectByDataUi).Click();
                // longer timeout, because DotVVM blocks postback queue for 5s after redirects to debounce any further requests
                increment(15000);
            });
        }
    }
}
