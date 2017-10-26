
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Testing.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class SPAViewModelReapplicationTests : AppSeleniumTest
    {
        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPAViewModelReapplication_pageA))]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPAViewModelReapplication_pageB))]
        public void Complex_SPAViewModelReapplication()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_SPAViewModelReapplication_pageA);
                browser.Wait(1000);

                // verify items count
                browser.FindElements("ul#first li").ThrowIfDifferentCountThan(3);
                browser.Single("#first")
                    .CheckIfInnerText(s => s.Contains("Entry 1") && s.Contains("Entry 2") && s.Contains("Entry 3"));

                browser.First("#test2").CheckIfInnerTextEquals("A");

                // verify first page values
                browser.First("input[type=text]").GetAttribute("value").Contains("Hello");
                browser.Last("input[type=text]").GetAttribute("value").Contains("1");


                //check url
                browser.CheckUrl(s => s.Contains("SPAViewModelReapplication/page"));

                // try the postback
                browser.First("input[type=button]").Click();
                browser.Wait();
                browser.First("#testResult").CheckIfInnerTextEquals("Hello1");

                // go to the second page
                browser.Single("#pageB").Click();
                browser.Wait();

                // verify items count and 
                browser.FindElements("ul#first li").ThrowIfDifferentCountThan(3);
                browser.Single("#first")
                    .CheckIfInnerText(s => s.Contains("Entry 1") && s.Contains("Entry 2") && s.Contains("Entry 3"));

                // verify second page values
                browser.First("input[type=text]").GetAttribute("value").Contains("World");
                browser.Last("input[type=text]").GetAttribute("value").Contains("2");
                browser.First("#test2").CheckIfInnerTextEquals("B");


                // try the postback
                browser.First("input[type=button]").Click();
                browser.Wait();
                browser.First("#testResult").CheckIfInnerTextEquals("World2");

                // go to first page
                browser.Single("#pageA").Click();
                browser.Wait();

                // verify items count
                browser.FindElements("ul#first li").ThrowIfDifferentCountThan(3);

                // verify first page values
                browser.First("input[type=text]").GetAttribute("value").Contains("Hello");
                browser.Last("input[type=text]").GetAttribute("value").Contains("1");
                browser.First("#test2").CheckIfInnerTextEquals("A");
            });
        }
    }
}
