using System;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;
using Xunit.Abstractions;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace DotVVM.Samples.Tests.Control
{
    public class RouteLinkTests : AppSeleniumTest
    {
        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_RouteLink_TestRoute))]
        public void Control_RouteLink_RouteLinkEnabled()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkEnabled);
                AssertUI.IsNotChecked(browser.Single("body > div.container > p:nth-child(2) > label > input[type=\"checkbox\"]"));
                browser.Single("body > div.container > p:nth-child(3) > a").Click();

                browser.Single("body > div.container > p:nth-child(2) > label > input[type=\"checkbox\"]").Click();
                browser.Single("body > div.container > p:nth-child(3) > a").Click();
                AssertUI.Url(browser, "/ControlSamples/Repeater/RouteLink/0", UrlKind.Relative, UriComponents.PathAndQuery);
                browser.NavigateBack();
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkUrlGen))]
        public void Control_RouteLink_RouteLinkUrlGeneration()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkUrlGen);

                CheckUrlGenerationMethod(browser);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkSpaUrlGen))]
        public void Control_RouteLink_RouteLinkSpaUrlGeneration()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkSpaUrlGen);

                CheckUrlGenerationMethod(browser, true);
            });
        }

        private static void CheckUrlGenerationMethod(IBrowserWrapper browser, bool isSpaLink = false)
        {
            void checkNavigatedUrl(string selector, string relativeUrl)
            {
                var href = browser.Single(selector).GetAttribute("href");

                Assert.AreEqual(relativeUrl, new Uri(href).AbsolutePath);
            }

            checkNavigatedUrl("a[data-ui='optional-parameter-client']", "/ControlSamples/Repeater/RouteLink");
            checkNavigatedUrl("a[data-ui='optional-parameter-server']", "/ControlSamples/Repeater/RouteLink");

            checkNavigatedUrl("a[data-ui='0-parameters-client']", "/");
            checkNavigatedUrl("a[data-ui='0-parameters-server']", "/");

            checkNavigatedUrl("a[data-ui='optional-parameter-prefixed-client']", "/ControlSamples/Repeater/RouteLink");
            checkNavigatedUrl("a[data-ui='optional-parameter-prefixed-server']", "/ControlSamples/Repeater/RouteLink");

            checkNavigatedUrl("a[data-ui='parameter-prefixed-client']", "/ControlSamples/Repeater/RouteLink/id-1");
            checkNavigatedUrl("a[data-ui='parameter-prefixed-server']", "/ControlSamples/Repeater/RouteLink/id-1");

            checkNavigatedUrl("a[data-ui='optional-parameter-at-start-client']", "/ControlSamples/Repeater/RouteLink");
            checkNavigatedUrl("a[data-ui='optional-parameter-at-start-server']", "/ControlSamples/Repeater/RouteLink");

            checkNavigatedUrl("a[data-ui='optional-prefixed-parameter-at-start-client']", "/id-1/ControlSamples/Repeater/RouteLink");
            checkNavigatedUrl("a[data-ui='optional-prefixed-parameter-at-start-client']", "/id-1/ControlSamples/Repeater/RouteLink");
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_RouteLink_TestRoute))]
        public void Control_RouteLink_RouteLinkEnabledFalse()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkEnabledFalse);

                //this RouteLink does not contain a binding (<dot:RouteLink Enabled="false" ... ) and should not redirect
                browser.First("a").Click();
                AssertUI.Url(browser, "/ControlSamples/RouteLink/RouteLinkEnabledFalse", UrlKind.Relative, UriComponents.PathAndQuery);

                //this RouteLink contains a binding ( <dot:RouteLink Enabled={{value: "false" ... }} and should not redirect
                browser.Last("a").Click();
                AssertUI.Url(browser, "/ControlSamples/RouteLink/RouteLinkEnabledFalse", UrlKind.Relative, UriComponents.PathAndQuery);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkQueryParameters))]
        public void Control_RouteLink_QueryParameters_DefaultValue()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkQueryParameters);

                browser.First(".link").Click();
                AssertUI.Url(browser,
                    u => u.EndsWith("/ControlSamples/RouteLink/RouteLinkQueryParameters?int=5&string=default")
                        || u.EndsWith("/ControlSamples/RouteLink/RouteLinkQueryParameters?string=default&int=5"));
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkQueryParameters))]
        public void Control_RouteLink_QueryParameters_CommandChangedValue()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkQueryParameters);

                browser.First(".command").Click();

                browser.First(".link").Click();
                AssertUI.Url(browser,
                    u => u.EndsWith("/ControlSamples/RouteLink/RouteLinkQueryParameters?int=7&string=change")
                         || u.EndsWith("/ControlSamples/RouteLink/RouteLinkQueryParameters?string=change&int=7"));
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkQueryParameters))]
        public void Control_RouteLink_QueryParameters_StaticCommandChangedValue()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkQueryParameters);

                browser.First(".static-command").Click();

                browser.First(".link").Click();
                AssertUI.Url(browser, u => u.EndsWith("/ControlSamples/RouteLink/RouteLinkQueryParameters?int=6&string=change_static")
                                           || u.EndsWith("/ControlSamples/RouteLink/RouteLinkQueryParameters?string=change_static&int=6"));
            });
        }

        public RouteLinkTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
