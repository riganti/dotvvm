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
    public class FormattingTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_Formatting()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Formatting_Formatting);

                var items1 = browser.FindElements(".list1 li");
                var items2 = browser.FindElements(".list2 li");

                items1.ElementAt(0).CheckIfInnerTextEquals(items2.ElementAt(0).GetText());
                items1.ElementAt(1).CheckIfInnerTextEquals(items2.ElementAt(1).GetText());
                items1.ElementAt(2).CheckIfInnerTextEquals(items2.ElementAt(2).GetText());

                //TODO: DateTime parse function for the third element

                items1.ElementAt(4).CheckIfInnerTextEquals(items2.ElementAt(4).GetText());
                items1.ElementAt(5).CheckIfInnerTextEquals(items2.ElementAt(5).GetText());
                items1.ElementAt(6).CheckIfInnerTextEquals(items2.ElementAt(6).GetText());
                items1.ElementAt(7).CheckIfInnerTextEquals(items2.ElementAt(7).GetText());
                items1.ElementAt(8).CheckIfInnerTextEquals(items2.ElementAt(8).GetText());
                items1.ElementAt(9).CheckIfInnerTextEquals(items2.ElementAt(9).GetText());
                items1.ElementAt(10).CheckIfInnerTextEquals(items2.ElementAt(10).GetText());
                items1.ElementAt(11).CheckIfInnerTextEquals(items2.ElementAt(11).GetText());
                items1.ElementAt(12).CheckIfInnerTextEquals(items2.ElementAt(12).GetText());
                items1.ElementAt(13).CheckIfInnerTextEquals(items2.ElementAt(13).GetText());
                
            });
        }
    }
}