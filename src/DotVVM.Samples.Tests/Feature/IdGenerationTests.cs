using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class IdGenerationTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_IdGeneration()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_IdGeneration_IdGeneration);
            
                browser.FindElements("*[data-id=test1_marker]").Single().CheckAttribute("id", s => s.Equals("test1"),
                    "Wrong ID");
                browser.FindElements("*[data-id=test2_marker]").Single().CheckAttribute("id", s => s.Equals("test2"),
                    "Wrong ID");

                browser.FindElements("*[data-id=test1a_marker]").Single().CheckAttribute("id", s => s.Equals("test1a"),
                    "Wrong ID");
                browser.FindElements("*[data-id=test2a_marker]").Single().CheckAttribute("id", s => s.Equals("test2a"),
                    "Wrong ID");
                
                var control1 = browser.FindElements("#ctl1").Single();
                control1.FindElements("*[data-id=control1_marker]").Single().CheckAttribute("id", s => s.Equals("ctl1_control1"),
                    "Wrong ID");
                control1.FindElements("*[data-id=control2_marker]").Single().CheckAttribute("id", s => s.Equals("ctl1_control2"),
                    "Wrong ID");

                var control2 = browser.FindElements("#ctl2").Single();
                control2.FindElements("*[data-id=control1_marker]").Single().CheckAttribute("id", s => s.Equals("control1"),
                    "Wrong ID");
                control2.FindElements("*[data-id=control2_marker]").Single().CheckAttribute("id", s => s.Equals("control2"),
                    "Wrong ID");

                var repeater1 = browser.FindElements("*[data-id=repeater1]").Single();
                for (int i = 0; i < 4; i++)
                {
                    repeater1.ElementAt("*[data-id=repeater1_marker]", i).CheckAttribute("id",
                        s => s.Equals(repeater1.GetAttribute("id") + "_i" + i + "_repeater1"),"Wrong ID");
                    repeater1.ElementAt("*[data-id=repeater2_marker]", i).CheckAttribute("id",
                        s => s.Equals(repeater1.GetAttribute("id") + "_i" + i + "_repeater2"), "Wrong ID");
                }

                var repeater2 = browser.FindElements("*[data-id=repeater2]").Single();
                for (int i = 0; i < 4; i++)
                {
                    repeater2.ElementAt("*[data-id=repeater1server_marker]", i).CheckAttribute("id",
                        s => s.Equals(repeater2.GetAttribute("id") + "_i" + i + "_repeater1server"), "Wrong ID");
                    repeater2.ElementAt("*[data-id=repeater2server_marker]", i).CheckAttribute("id",
                        s => s.Equals(repeater2.GetAttribute("id") + "_i" + i + "_repeater2server"), "Wrong ID");
                }
            });
        }
    }
}