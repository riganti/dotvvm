using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class GridViewTests : SeleniumTestBase
    {


        [TestMethod]
        public void Control_GridViewPagingSorting()
        {
            Control_GridViewPagingSortingBase( SamplesRouteUrls.ControlSamples_GridView_GridViewPagingSorting);

        }
        [TestMethod]
        public void Control_GridViewServerRender()
        {
            Control_GridViewPagingSortingBase(SamplesRouteUrls.ControlSamples_GridView_GridViewServerRender);
        }

        [TestMethod]
        public void Control_GridViewStaticCommand()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewStaticCommand);
                //check rows
                browser.FindElements("table tbody tr").ThrowIfDifferentCountThan(5);
                //check first row Id
                browser.First("table tbody tr td span").CheckIfInnerTextEquals("1");
                //cal static command for delete row
                browser.First("table tbody tr input[type=button]").Click();
                //check rows again
                browser.FindElements("table tbody tr").ThrowIfDifferentCountThan(4);
                //check first row Id
                browser.First("table tbody tr td span").CheckIfInnerTextEquals("2");
            });
        }

        public void Control_GridViewPagingSortingBase(string path) { 

            
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(path);

                Action performTest = () =>
                {
                    //// make sure that thirs row's first cell is yellow
                    browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0).CheckClassAttribute(s => s.Equals(""));
                    browser.ElementAt("table", 0).ElementAt("tr", 2).ElementAt("td", 0).CheckClassAttribute(s => s.Equals("alternate"));

                    //// go to second page
                    browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0).CheckIfInnerTextEquals("1");
                    browser.ElementAt("ul", 0).FindElements("li a").Single(s => s.GetText() == "2").Click();
                    browser.Wait();

                    //// go to previous page
                    browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0).CheckIfInnerTextEquals("11");
                    browser.ElementAt("ul", 0).FindElements("li a").Single(s => s.GetText() == "««").Click();
                    browser.Wait();

                    //// go to next page
                    browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0).CheckIfInnerTextEquals("1");
                    browser.ElementAt("ul", 0).FindElements("li a").Single(s => s.GetText() == "»»").Click();
                    browser.Wait();

                    //// try the disabled link - nothing should happen
                    browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0).CheckIfInnerTextEquals("11");
                    browser.ElementAt("ul", 0).FindElements("li a").Single(s => s.GetText() == "»»").Click();
                    browser.Wait();

                    browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0).CheckIfInnerTextEquals("11");

                    // try sorting in the first grid
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 2).ElementAt("a", 0).Click();
                    browser.Wait();
                    browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0).CheckIfInnerTextEquals("4");

                    //// sort descending in the first grid
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 2).ElementAt("a", 0).Click();
                    browser.Wait();
                    browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0).CheckIfInnerTextEquals("9");

                    //// sort by different column in the first grid
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 0).ElementAt("a", 0).Click();
                    browser.Wait();
                    browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0).CheckIfInnerTextEquals("1");

                    //// try sorting in the first grid
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 2).ElementAt("a", 0).Click();
                    browser.Wait();
                    browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0).CheckIfInnerTextEquals("4");

                    //// sort by different column in the first grid
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 0).ElementAt("a", 0).Click();
                    browser.Wait();
                    browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0).CheckIfInnerTextEquals("1");
                };

                browser.Wait();
                performTest();
                browser.Wait();
                browser.NavigateToUrl();
                browser.Wait();
                browser.NavigateBack();
                browser.Wait();
                performTest();
            });
        }

      
    }
}
