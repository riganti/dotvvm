using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests
{
    public class SpaNavigationToEmptyUrlTests : AppSeleniumTest
    {

        [Fact]
        [SampleReference(SamplesRouteUrls.SpaNavigationToEmptyUrl)]
        public void SpaNavigationToEmptyUrlTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.SpaNavigationToEmptyUrl);
                browser.Wait();

                browser.Single("a").Click().Wait();

                AssertUI.UrlEquals(browser, browser.BaseUrl);
            });
        }


        [Fact]
        [SampleReference(SamplesRouteUrls.SpaNavigationToEmptyUrl)]
        public void SpaNavigationToEmptyUrlTest_Redirect()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.SpaNavigationToEmptyUrl);
                browser.Wait();

                browser.Single("input[type=button]").Click().Wait();

                AssertUI.UrlEquals(browser, browser.BaseUrl);
            });
        }


        public SpaNavigationToEmptyUrlTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
