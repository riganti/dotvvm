using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class RouteLinkTests : SeleniumTest
    {
        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_RouteLink_TestRoute))]
        public void Control_RouteLink_RouteLinkEnabled()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkEnabled);
                browser.Single("body > div.container > p:nth-child(2) > label > input[type=\"checkbox\"]")
                    .CheckIfIsNotChecked();
                browser.Single("body > div.container > p:nth-child(3) > a").Click();

                browser.Single("body > div.container > p:nth-child(2) > label > input[type=\"checkbox\"]").Click();
                browser.Single("body > div.container > p:nth-child(3) > a").Click();
                browser.CompareUrl("http://localhost:60320/ControlSamples/Repeater/RouteLink/0");
            });
        }

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_RouteLink_TestRoute))]
        public void Control_RouteLink_RouteLinkEnabledFalse()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RouteLink_RouteLinkEnabledFalse);

                //this RouteLink does not contain a binding (<dot:RouteLink Enabled="false" ... ) and should not redirect
                browser.First("a").Click();
                browser.CompareUrl("http://localhost:60320/ControlSamples/RouteLink/RouteLinkEnabledFalse");

                //this RouteLink contains a binding ( <dot:RouteLink Enabled={{value: "false" ... }} and should not redirect
                browser.Last("a").Click();
                browser.CompareUrl("http://localhost:60320/ControlSamples/RouteLink/RouteLinkEnabledFalse");
            });
        }
    }
}
