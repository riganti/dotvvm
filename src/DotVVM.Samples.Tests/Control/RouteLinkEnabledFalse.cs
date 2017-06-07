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
    public class RouteLinkEnabledFalse :SeleniumTest
    {
        [TestMethod]
        public void RouteLinkEnabledFalseTest()
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
