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

        [TestMethod]
        public void Control_GridViewInlineEditingServer()
        {
            Control_GridViewInlineEditing(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditing, 0);
        }

        [TestMethod]
        public void Control_GridViewInlineEditingClient()
        {
            Control_GridViewInlineEditing(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditing, 1);
        }

        public void Control_GridViewInlineEditing(string path, int tableID)
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(path);

                // get table
                var table = browser.ElementAt("table", tableID);
                
                //check rows
                table.FindElements("tbody tr").ThrowIfDifferentCountThan(10);

                // check whether the first row is in edit mode
                var firstRow = table.First("tbody tr");
                firstRow.First("td").CheckIfInnerTextEquals("1");
                firstRow.ElementAt("td", 1).Single("input").CheckIfIsDisplayed();
                firstRow.ElementAt("td", 2).Single("input").CheckIfIsDisplayed();
                firstRow.ElementAt("td", 3).FindElements("button").ThrowIfDifferentCountThan(2);

                // check if right number of testboxs are displayed => IsEditable works
                table.FindElements("tbody tr td input").ThrowIfDifferentCountThan(2);

                // click on Cancel button
                firstRow.ElementAt("td", 3).ElementAt("button", 1).Click();

                // click the Edit button on another row
                table = browser.ElementAt("table", tableID);
                var desiredRow = table.ElementAt("tbody tr", 3);
                desiredRow.ElementAt("td", 3).Single("button").Click();

                // check if edit row changed
                table = browser.ElementAt("table", tableID);
                desiredRow = table.ElementAt("tbody tr", 3);
                desiredRow.First("input").CheckIfIsDisplayed();
                desiredRow.FindElements("button").ThrowIfDifferentCountThan(2);
            });
        }
        public void Control_GridViewPagingSortingBase(string path) { 

            
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(path);
                browser.ActionWaitTime = 500;

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
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 2).ElementAt("button", 0).Click();
                    browser.Wait();
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 1).ElementAt("a", 0).Click();
                    browser.Wait();
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 1).CheckClassAttribute("sort-asc");

                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 0).ElementAt("a", 0).Click();
                    browser.Wait();
                    browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0).CheckIfInnerTextEquals("1");

                    //// sort descending in the first grid
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 1).ElementAt("a", 0).Click();
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 1).ElementAt("a", 0).Click();
                    browser.Wait();
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 1).CheckClassAttribute("sort-desc");
                    browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0).CheckIfInnerTextEquals("16");

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

        [TestMethod]
        public void Control_GridViewRowDecorators()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewRowDecorators);

                browser.FindElements("tr").ThrowIfDifferentCountThan(6);

                browser.ElementAt("tr", 3).Click();
                for (int i = 0; i < 6; i++)
                {
                    browser.ElementAt("tr", i).CheckClassAttribute(v => v.Contains("selected") == (i == 3));
                }

                browser.ElementAt("tr", 2).Click();
                for (int i = 0; i < 6; i++)
                {
                    browser.ElementAt("tr", i).CheckClassAttribute(v => v.Contains("selected") == (i == 2));
                }
            });
        }

    }
}
