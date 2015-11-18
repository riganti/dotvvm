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
    public class CascadeSelectorsTests : SeleniumTestBase
    {
        [TestMethod]
        public void Complex_CascadeSelectors(string url = SamplesRouteUrls.ComplexSamples_CascadeSelectors)
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(url);

                // select city
                browser.First("select").Select(1);
                browser.First("input[type=button]").Click();
                browser.Wait();

                // select hotel
                browser.Last("select").Select(1);
                browser.Last("input[type=button]").Click();
                browser.Wait();

                browser.First("h2").CheckIfInnerTextEquals("Hotel Seattle #2");

                // select city
                browser.First("select").Select(0);
                browser.First("input[type=button]").Click();
                browser.Wait();

                // select hotel
                browser.Last("select")
                    .First("option").Click();

                browser.Last("input[type=button]").Click();
                browser.Wait();

                browser.First("h2").CheckIfInnerTextEquals("Hotel Prague #1");
            });
        }

        [TestMethod]
        public void Complex_CascadeSelectorsServerRender()
        {
            Complex_CascadeSelectors(SamplesRouteUrls.ComplexSamples_CascadeSelectorsServerRender);
        }
    }
}
