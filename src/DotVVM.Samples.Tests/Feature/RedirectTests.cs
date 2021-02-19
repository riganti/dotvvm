using System;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
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
                currentUrl = new Uri(browser.CurrentUrl);
                Assert.Matches(@"^\?(param=temp1&time=\d+|time=\d+&param=temp1)$", currentUrl.Query);
                Assert.Equal("#test1", currentUrl.Fragment);

                browser.First("[data-ui=dictionary-redirect-button]").Click();
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
        
    }
}
