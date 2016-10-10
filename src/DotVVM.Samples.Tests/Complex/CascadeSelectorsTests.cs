using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;
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
        public void Complex_CascadeSelectorsServerRender()
        {
            Complex_CascadeSelectorsBase(SamplesRouteUrls.ComplexSamples_CascadeSelectors_CascadeSelectorsServerRender);
        }

        [TestMethod]
        public void Complex_CascadeSelectors()
        {
            Complex_CascadeSelectorsBase(SamplesRouteUrls.ComplexSamples_CascadeSelectors_CascadeSelectors);
        }

        public void Complex_CascadeSelectorsBase(string url)
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(url);

                // select city
                browser.First("select").Select(1);
                browser.First("input[type=button]").Click().Wait();

                // select hotel
                browser.Last("select").Select(1);
                browser.Last("input[type=button]").Click().Wait();

                browser.First("h2").CheckIfInnerTextEquals("Hotel Seattle #2");

                // select city
                browser.First("select").Select(0);
                browser.First("input[type=button]").Click().Wait();

                // select hotel
                browser.Last("select").Select(0);
                browser.Last("input[type=button]").Click().Wait();

                browser.First("h2").CheckIfInnerTextEquals("Hotel Prague #1");
            });
        }
    }
}
