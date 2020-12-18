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
    public class PostbackSpaNavigationTests : AppSeleniumTest
    {

        [SampleReference(SamplesRouteUrls.FeatureSamples_PostbackSpaNavigation_PageA)]
        [SampleReference(SamplesRouteUrls.FeatureSamples_PostbackSpaNavigation_PageB)]
        [Fact]
        public void PostbackSpaNavigationTest_SuccessfulNavigation()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackSpaNavigation_PageA);

                var links = browser.FindElements("a");
                var buttons = browser.FindElements("input[type=button]");
                var result = browser.Single(".result");

                // click the button and make sure the postback works
                AssertUI.TextEquals(result, "0");
                buttons[0].Click().Wait();
                AssertUI.TextEquals(result, "1");
                buttons[1].Click().Wait();
                AssertUI.TextEquals(result, "2");

                // click the first link to trigger the navigation
                links[0].Click();

                buttons[0].Click().Wait();
                AssertUI.TextEquals(result, "2");
                buttons[1].Click().Wait();
                AssertUI.TextEquals(result, "2");

                browser.Wait(3000);

                AssertUI.UrlEquals(browser, browser.BaseUrl + SamplesRouteUrls.FeatureSamples_PostbackSpaNavigation_PageB);
            });
        }

        [SampleReference(SamplesRouteUrls.FeatureSamples_PostbackSpaNavigation_PageA)]
        [Fact]
        public void PostbackSpaNavigationTest_FailedNavigation()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackSpaNavigation_PageA);

                var links = browser.FindElements("a");
                var buttons = browser.FindElements("input[type=button]");
                var result = browser.Single(".result");

                // click the button and make sure the postback works
                AssertUI.TextEquals(result, "0");
                buttons[0].Click().Wait();
                AssertUI.TextEquals(result, "1");
                buttons[1].Click().Wait();
                AssertUI.TextEquals(result, "2");

                // click the second link to trigger the navigation
                links[1].Click();

                // now the buttons shouldn't do anything
                buttons[0].Click().Wait();
                AssertUI.TextEquals(result, "2");
                buttons[1].Click().Wait();
                AssertUI.TextEquals(result, "2");

                browser.Wait(3000);

                // dismiss the error window
                AssertUI.IsDisplayed(browser.Single("#debugWindow"));
                browser.Single("#debugWindow button").Click();

                // now the buttons should work
                buttons[0].Click().Wait();
                AssertUI.TextEquals(result, "3");
                buttons[1].Click().Wait();
                AssertUI.TextEquals(result, "4");
            });
        }

        [SampleReference(SamplesRouteUrls.FeatureSamples_PostbackSpaNavigation_PageA)]
        [SampleReference(SamplesRouteUrls.FeatureSamples_PostbackSpaNavigation_PageB)]
        [Fact]
        public void PostbackSpaNavigationTest_SuccessfulNavigation_SurvivingCommand()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackSpaNavigation_PageA);

                var links = browser.FindElements("a");
                var buttons = browser.FindElements("input[type=button]");
                var result = browser.Single(".result");

                // click the button to start a long postback
                AssertUI.TextEquals(result, "0");
                buttons[2].Click().Wait();

                // click the first link to trigger the navigation
                links[0].Click();

                // wait for the navigation and postback to finish
                browser.Wait(6000);
                AssertUI.UrlEquals(browser, browser.BaseUrl + SamplesRouteUrls.FeatureSamples_PostbackSpaNavigation_PageB);

                // check that the new field was not incremented
                result = browser.Single(".result");
                AssertUI.TextEquals(result, "0");
            });
        }

        [SampleReference(SamplesRouteUrls.FeatureSamples_PostbackSpaNavigation_PageA)]
        [SampleReference(SamplesRouteUrls.FeatureSamples_PostbackSpaNavigation_PageB)]
        [Fact]
        public void PostbackSpaNavigationTest_SuccessfulNavigation_SurvivingStaticCommand()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackSpaNavigation_PageA);

                var links = browser.FindElements("a");
                var buttons = browser.FindElements("input[type=button]");
                var result = browser.Single(".result");

                // click the button to start a long postback
                AssertUI.TextEquals(result, "0");
                buttons[3].Click().Wait();

                // click the first link to trigger the navigation
                links[0].Click();

                // wait for the navigation and postback to finish
                browser.Wait(6000);
                AssertUI.UrlEquals(browser, browser.BaseUrl + SamplesRouteUrls.FeatureSamples_PostbackSpaNavigation_PageB);

                // check that the new field was not incremented
                result = browser.Single(".result");
                AssertUI.TextEquals(result, "0");
            });
        }

        public PostbackSpaNavigationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
