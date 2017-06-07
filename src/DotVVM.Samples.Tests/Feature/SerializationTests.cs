using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class SerializationTests : SeleniumTest
    {
        [TestMethod]
        public void Feature_Serialization()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Serialization_Serialization);

                // fill the values
                browser.ElementAt("input[type=text]", 0).SendKeys("1");
                browser.ElementAt("input[type=text]", 1).SendKeys("2");
                browser.Click("input[type=button]");

                // verify the results
                browser.ElementAt("input[type=text]", 0).CheckAttribute("value", s => s.Equals(""));
                browser.ElementAt("input[type=text]", 1).CheckAttribute("value", s => s.Equals("2"));
                browser.Last("span").CheckIfInnerTextEquals(",2");
            });
        }


        [TestMethod]
        public void Feature_Serialization_ObservableCollectionShouldContainObservables()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Serialization_ObservableCollectionShouldContainObservables);
                browser.Wait();
                
                // verify that the values are selected
                browser.ElementAt("select", 0).Select(0);
                browser.ElementAt("select", 1).Select(1);
                browser.ElementAt("select", 2).Select(2);

                // click the button
                browser.Click("input[type=button]");

                // verify that the values are correct
                browser.First("p.result").CheckIfInnerTextEquals("1,2,3");
                browser.ElementAt("select", 0).CheckAttribute("value", "1");
                browser.ElementAt("select", 1).CheckAttribute("value", "2");
                browser.ElementAt("select", 2).CheckAttribute("value", "3");
                browser.Wait();

                // change the values
                browser.ElementAt("select", 0).Select(1);
                browser.ElementAt("select", 1).Select(2);
                browser.ElementAt("select", 2).Select(1);

                // click the button
                browser.Click("input[type=button]");

                // verify that the values are correct
                browser.First("p.result").CheckIfInnerTextEquals("2,3,2");
                browser.ElementAt("select", 0).CheckAttribute("value", "2");
                browser.ElementAt("select", 1).CheckAttribute("value", "3");
                browser.ElementAt("select", 2).CheckAttribute("value", "2");
            });
        }
    }
}