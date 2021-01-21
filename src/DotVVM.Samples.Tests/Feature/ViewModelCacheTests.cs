using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class ViewModelCacheTests : AppSeleniumTest
    {
        public ViewModelCacheTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_ViewModelCache_ViewModelCacheMiss()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ViewModelCache_ViewModelCacheMiss);
                browser.Wait();

                var cacheEnabled = browser.Single(".cacheEnabled").GetText() == "True";

                var result = browser.Single(".result");
                var requestCount = browser.Single(".requestCount");
                AssertUI.TextEquals(result, "0");
                AssertUI.TextEquals(requestCount, "0");

                // normal postback
                browser.ElementAt("input[type=button]", 0).Click();
                browser.WaitFor(() => {
                    AssertUI.TextEquals(result, "1");
                    AssertUI.TextEquals(requestCount, "1");
                }, 5000);

                // tamper with viewmodel cache id - it should do two requests but it should still work
                browser.ElementAt("input[type=button]", 1).Click();
                browser.ElementAt("input[type=button]", 0).Click();

                browser.WaitFor(() => {
                    if (cacheEnabled)
                    {
                        AssertUI.TextEquals(result, "2");
                        AssertUI.TextEquals(requestCount, "3");

                        // normal postback
                        browser.ElementAt("input[type=button]", 0).Click().Wait(1000);
                        AssertUI.TextEquals(result, "3");
                        AssertUI.TextEquals(requestCount, "4");
                    }
                    else
                    {
                        AssertUI.IsDisplayed(browser.FindElements("#debugWindow").First());
                    }
                }, 5000);
            });
        }
    }
}
