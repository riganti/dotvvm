using System;
using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_RouteLink_TestRoute))]
        public void Control_RouteLink_RouteLinkUrlGeneration()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkUrlGen);

                browser.Single("a[data-ui='optional-parameter-client']").Click();
                browser.CheckUrl("/ControlSamples/Repeater/RouteLink", UrlKind.Relative, UriComponents.PathAndQuery);
                browser.NavigateBack();

                browser.Single("a[data-ui='optional-parameter-server']").Click();
                browser.CheckUrl("/ControlSamples/Repeater/RouteLink", UrlKind.Relative, UriComponents.PathAndQuery);
                browser.NavigateBack();

                browser.Single("a[data-ui='0-parameters']").Click();
                browser.CheckUrl("/", UrlKind.Relative, UriComponents.PathAndQuery);
                browser.NavigateBack();
            });
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
