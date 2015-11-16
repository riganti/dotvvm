using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;
using DotVVM.Framework.Routing;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class SerializationTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_Serialization()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("FeatureSamples/Serialization/Serialization");

                // fill the values
                browser.ElementAt("input[type=text]", 0).SendKeys("1");
                browser.ElementAt("input[type=text]", 1).SendKeys("2");
                browser.Click("input[type=button]");
                browser.Wait();

                // verify the results
                browser.ElementAt("input[type=text]", 0).CheckAttribute("value", s => s.Equals(""));
                browser.ElementAt("input[type=text]", 1).CheckAttribute("value", s => s.Equals("2"));
                browser.Last("span").CheckIfInnerTextEquals(",2");
            });
        }
    }
}