using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class RouteLinkEnabledTests : SeleniumTestBase  
    {
        [TestMethod]
        public void RouteLinkEnabledTest()
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
    }
}
