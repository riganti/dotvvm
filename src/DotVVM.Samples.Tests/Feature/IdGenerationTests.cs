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
                browser.Wait();

                browser.Single("*[data-id=test1_marker]").CheckAttribute("id", s => s.Equals("test1"),
                    "Wrong ID");
                browser.Single("*[data-id=test2_marker]").CheckAttribute("id", s => s.Equals("test2"),
                    "Wrong ID");

                browser.Single("*[data-id=test1a_marker]").CheckAttribute("id", s => s.Equals("test1a"),
                    "Wrong ID");
                browser.Single("*[data-id=test2a_marker]").CheckAttribute("id", s => s.Equals("test2a"),
                    "Wrong ID");
                
                var control1 = browser.Single("#ctl1");
                control1.Single("*[data-id=control1_marker]").CheckAttribute("id", s => s.Equals("ctl1_control1"),
                    "Wrong ID");
                control1.Single("*[data-id=control2_marker]").CheckAttribute("id", s => s.Equals("ctl1_control2"),
                    "Wrong ID");

                var control2 = browser.Single("#ctl2");
                control2.Single("*[data-id=control1_marker]").CheckAttribute("id", s => s.Equals("control1"),
                    "Wrong ID");
                control2.Single("*[data-id=control2_marker]").CheckAttribute("id", s => s.Equals("control2"),
                    "Wrong ID");

                var repeater1 = browser.Single("*[data-id=repeater1]");
                for (int i = 0; i < 4; i++)
                {
                    repeater1.ElementAt("*[data-id=repeater1_marker]", i).CheckAttribute("id",
                        s => s.Equals(repeater1.GetAttribute("id") + "_i" + i + "_repeater1"), "Wrong ID");
                    repeater1.ElementAt("*[data-id=repeater2_marker]", i).CheckAttribute("id",
                        s => s.Equals(repeater1.GetAttribute("id") + "_i" + i + "_repeater2"), "Wrong ID");
                }

                var repeater2 = browser.Single("*[data-id=repeater2]");
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