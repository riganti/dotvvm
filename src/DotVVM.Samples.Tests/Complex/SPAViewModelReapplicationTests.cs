using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class SPAViewModelReapplicationTests : SeleniumTestBase
    {
        [TestMethod]
        public void Complex_SPAViewModelReapplication()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_SPAViewModelReapplication);
                browser.Wait();

                // verify items count
                browser.FindElements("ul#first li").ThrowIfDifferentCountThan(3);

                // verify first page values
                browser.First("input[type=text]").GetAttribute("value").Contains("Hello");
                browser.Last("input[type=text]").GetAttribute("value").Contains("1");
                browser.First("#test2").CheckIfInnerTextEquals("A");

                // try the postback
                browser.First("input[type=button]").Click();
                browser.Wait();
                browser.First("#testResult").CheckIfInnerTextEquals("Hello1");

                // go to second page
                browser.Last("a").Click();
                browser.Wait();

                // verify items count
                browser.FindElements("ul#first li").ThrowIfDifferentCountThan(3);

                // verify second page values
                browser.First("input[type=text]").GetAttribute("value").Contains("World");
                browser.Last("input[type=text]").GetAttribute("value").Contains("2");
                browser.First("#test2").CheckIfInnerTextEquals("B");

                // try the postback
                browser.First("input[type=button]").Click();
                browser.Wait();
                browser.First("#testResult").CheckIfInnerTextEquals("World2");

                // go to first page
                browser.First("a").Click();
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
