using System;
using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class RouteLinkTests : AppSeleniumTest
    {
        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_RouteLink_TestRoute))]
        public void Control_RouteLink_RouteLinkEnabled()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkEnabled);
                browser.Single("body > div.container > p:nth-child(2) > label > input[type=\"checkbox\"]")
                    .CheckIfIsNotChecked();
                browser.Single("body > div.container > p:nth-child(3) > a").Click();

                browser.Single("body > div.container > p:nth-child(2) > label > input[type=\"checkbox\"]").Click();
                browser.Single("body > div.container > p:nth-child(3) > a").Click();
                browser.CheckUrl("/ControlSamples/Repeater/RouteLink/0", UrlKind.Relative, UriComponents.PathAndQuery);
                browser.NavigateBack();
            });
        }

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkUrlGen))]
        public void Control_RouteLink_RouteLinkUrlGeneration()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkUrlGen);

                CheckUrlGenerationMethod(browser);
            });
        }

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkSpaUrlGen))]
        public void Control_RouteLink_RouteLinkSpaUrlGeneration()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkSpaUrlGen);

                CheckUrlGenerationMethod(browser, true);
            });
        }

        private static void CheckUrlGenerationMethod(IBrowserWrapperFluentApi browser, bool isSpaLink = false)
        {
            void checkNavigatedUrl(string selector, string relativeUrl)
            {
                var href = browser.Single(selector).GetAttribute("href");
                if (isSpaLink)
                {
                    Assert.AreEqual("#!" + relativeUrl, new Uri(href).Fragment);
                }
                else
                {
                    Assert.AreEqual(relativeUrl, new Uri(href).AbsolutePath);
                }
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

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_RouteLink_TestRoute))]
        public void Control_RouteLink_RouteLinkEnabledFalse()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkEnabledFalse);

                //this RouteLink does not contain a binding (<dot:RouteLink Enabled="false" ... ) and should not redirect
                browser.First("a").Click();
                browser.CheckUrl("/ControlSamples/RouteLink/RouteLinkEnabledFalse", UrlKind.Relative, UriComponents.PathAndQuery);

                //this RouteLink contains a binding ( <dot:RouteLink Enabled={{value: "false" ... }} and should not redirect
                browser.Last("a").Click();
                browser.CheckUrl("/ControlSamples/RouteLink/RouteLinkEnabledFalse", UrlKind.Relative, UriComponents.PathAndQuery);
            });
        }
    }
}
