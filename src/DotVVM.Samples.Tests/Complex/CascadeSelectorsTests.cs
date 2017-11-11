
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Testing.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class CascadeSelectorsTests : AppSeleniumTest
    {
        [TestMethod]
        public void Complex_CascadeSelectors_CascadeSelectors()
        {
            Complex_CascadeSelectorsBase(SamplesRouteUrls.ComplexSamples_CascadeSelectors_CascadeSelectors);
        }

        [TestMethod]
        public void Complex_CascadeSelectors_CascadeSelectorsServerRender()
        {
            Complex_CascadeSelectorsBase(SamplesRouteUrls.ComplexSamples_CascadeSelectors_CascadeSelectorsServerRender);
        }

        [TestMethod]
        public void Complex_CascadeSelectors_TripleComboBoxes()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_CascadeSelectors_TripleComboBoxes);
                browser.ElementAt("input[type=button]", 0).Click().Wait();
                browser.ElementAt(".active", 0).CheckIfInnerTextEquals("North America: 1");
                browser.ElementAt(".active", 1).CheckIfInnerTextEquals("USA: 11");
                browser.ElementAt(".active", 2).CheckIfInnerTextEquals("New York: 111");

                browser.ElementAt("input[type=button]", 2).Click().Wait();
                browser.ElementAt(".active", 0).CheckIfInnerTextEquals("North America: 1");
                browser.ElementAt(".active", 1).CheckIfInnerTextEquals("Canada: 12");
                browser.ElementAt(".active", 2).CheckIfInnerTextEquals("Toronto: 121");

                browser.ElementAt("input[type=button]", 5).Click().Wait();
                browser.ElementAt(".active", 0).CheckIfInnerTextEquals("Europe: 2");
                browser.ElementAt(".active", 1).CheckIfInnerTextEquals("Germany: 21");
                browser.ElementAt(".active", 2).CheckIfInnerTextEquals("Munich: 212");

                browser.ElementAt("input[type=button]", 8).Click().Wait();
                browser.ElementAt(".active", 0).CheckIfInnerTextEquals("Asia: 3");
                browser.ElementAt(".active", 1).CheckIfInnerTextEquals("China: 31");
                browser.ElementAt(".active", 2).CheckIfInnerTextEquals("Beijing: 311");
            });
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
