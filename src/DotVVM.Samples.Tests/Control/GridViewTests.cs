using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class GridViewTests : SeleniumTest
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
                browser.Wait();

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
        public void Control_GridViewInlineEditingValidation()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditingValidation);
                browser.Browser.Manage().Window.Maximize();

                //Get rows
                var rows = browser.First("table tbody");
                var firstRow = rows.ElementAt("tr", 0);

                //Edit
                firstRow.ElementAt("td", 5).First("button").Click();

                rows = browser.First("table tbody");
                firstRow = rows.ElementAt("tr", 0);

                //Check type
                firstRow.ElementAt("td", 1).First("input").CheckAttribute("type", "text");
                firstRow.ElementAt("td", 2).First("input").CheckAttribute("type", "text");
                firstRow.ElementAt("td", 3).First("input").CheckAttribute("type", "text");
                firstRow.ElementAt("td", 4).First("input").CheckAttribute("type", "text");

                //clear name
                firstRow.ElementAt("td", 1).First("input").Clear();

                //update buttons
                firstRow.ElementAt("td", 5).FindElements("button").ThrowIfDifferentCountThan(2);

                //update
                firstRow.ElementAt("td", 5).First("button").Click();

                //getting rid iof "postback interupted message"
                browser.FindElements("div#debugNotification").First().Click();
                browser.Wait(1000);

                var validationResult = browser.ElementAt(".validation", 0);
                validationResult.CheckIfInnerTextEquals("The Name field is required.");

                //change name
                firstRow.ElementAt("td", 1).First("input").SendKeys("Test");

                //clear email
                firstRow.ElementAt("td", 3).First("input").Clear();
                
                //update
                firstRow.ElementAt("td", 5).First("button").Click();

                //check validation
                validationResult.CheckIfInnerTextEquals("The Email field is not a valid e-mail address.");
            });
        }

        [TestMethod]
        public void Control_GridViewInlineEditingFormat()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditingValidation);
                
                //Get rows
                var rows = browser.First("table tbody");
                rows.FindElements("tr").ThrowIfDifferentCountThan(3);

                var firstRow = rows.ElementAt("tr", 0);
                var dateDisplay = firstRow.ElementAt("td", 2).First("span").GetText();
                var moneyDisplay = firstRow.ElementAt("td", 4).First("span").GetText();

                //Edit
                firstRow.ElementAt("td", 5).First("button").Click();
                browser.Wait(500);

                //init again
                rows = browser.First("table tbody");
                firstRow = rows.ElementAt("tr", 0);

                //check format
                firstRow.ElementAt("td", 2).First("input").CheckIfTextEquals(dateDisplay);
                firstRow.ElementAt("td", 4).First("input").CheckIfTextEquals(moneyDisplay);

            });
        }

        [TestMethod]
        public void Control_GridViewInlineEditingPrimaryKeyGuid()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditingPrimaryKeyGuid);
                //Get rows
                var rows = browser.First("table tbody");
                rows.FindElements("tr").ThrowIfDifferentCountThan(3);

                var firstRow = rows.ElementAt("tr",0);
                firstRow.ElementAt("td", 0).First("span").CheckIfInnerTextEquals("9536d712-2e91-43d2-8ebb-93fbec31cf34");
                //Edit
                firstRow.ElementAt("td", 4).First("button").Click();
                browser.Wait(500);

                //init again
                rows = browser.First("table tbody");
                firstRow = rows.ElementAt("tr", 0);

                //Check type
                firstRow.ElementAt("td", 1).First("input").CheckAttribute("type","text");
                firstRow.ElementAt("td", 2).First("input").CheckAttribute("type", "text");
                firstRow.ElementAt("td", 3).First("input").CheckAttribute("type", "text");

                //change name
                firstRow.ElementAt("td", 1).First("input").Clear();
                firstRow.ElementAt("td", 1).First("input").SendKeys("Test");

                //update buttons
                firstRow.ElementAt("td", 4).FindElements("button").ThrowIfDifferentCountThan(2);

                //update
                firstRow.ElementAt("td", 4).First("button").Click();
                browser.Wait(500);

                //init again
                rows = browser.First("table tbody");
                firstRow = rows.ElementAt("tr", 0);

                //check changed name
                firstRow.ElementAt("td", 1).First("span").CheckIfInnerTextEquals("Test");
               
            });
        }


        [TestMethod]
        public void Control_GridViewInlineEditingPrimaryKeyString()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditingPrimaryKeyString);
                //Get rows
                var rows = browser.First("table tbody");
                rows.FindElements("tr").ThrowIfDifferentCountThan(3);

                var firstRow = rows.ElementAt("tr", 0);
                firstRow.ElementAt("td", 0).First("span").CheckIfInnerTextEquals("A");
                //Edit
                firstRow.ElementAt("td", 4).First("button").Click();
                browser.Wait(500);

                //init again
                rows = browser.First("table tbody");
                firstRow = rows.ElementAt("tr", 0);

                //Check type
                firstRow.ElementAt("td", 1).First("input").CheckAttribute("type", "text");
                firstRow.ElementAt("td", 2).First("input").CheckAttribute("type", "text");
                firstRow.ElementAt("td", 3).First("input").CheckAttribute("type", "text");

                //change name
                firstRow.ElementAt("td", 1).First("input").Clear();
                firstRow.ElementAt("td", 1).First("input").SendKeys("Test");

                //update buttons
                firstRow.ElementAt("td", 4).FindElements("button").ThrowIfDifferentCountThan(2);

                //update
                firstRow.ElementAt("td", 4).First("button").Click();
                browser.Wait(500);

                //init again
                rows = browser.First("table tbody");
                firstRow = rows.ElementAt("tr", 0);

                //check changed name and Id
                firstRow.ElementAt("td", 0).First("span").CheckIfInnerTextEquals("A");
                firstRow.ElementAt("td", 1).First("span").CheckIfInnerTextEquals("Test");

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

        [TestMethod]
        public void Control_GridViewInlineEditingPagingWhenEditModeServer()
        {
            Control_GridViewInlineEditingPagingWhenEditing(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditing, 0);
        }

        [TestMethod]
        public void Control_GridViewInlineEditingPagingWhenEditModeClient()
        {
            Control_GridViewInlineEditingPagingWhenEditing(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditing, 1);
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
                firstRow.ElementAt("td", 3).ElementAt("button", 1).ScrollTo().Click();
                browser.Wait(500);

                // click the Edit button on another row
                table = browser.ElementAt("table", tableID);
                var desiredRow = table.ElementAt("tbody tr", 3);
                desiredRow.ElementAt("td", 3).Single("button").ScrollTo().Click();
                browser.Wait(500);

                // check if edit row changed
                table = browser.ElementAt("table", tableID);
                desiredRow = table.ElementAt("tbody tr", 3);
                desiredRow.First("input").CheckIfIsDisplayed();
                desiredRow.FindElements("button").ThrowIfDifferentCountThan(2);
            });
        }

        public void Control_GridViewInlineEditingPagingWhenEditing(string path, int tableID)
        {
            RunInAllBrowsers(browser =>
            {
                browser.Refresh();
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

                //page to second page
                var navigation = browser.ElementAt(".pagination", 0);
                navigation.FindElements("li a").Single(s => s.GetText() == "2").Click();
                browser.Wait(500);

                table = browser.ElementAt("table", tableID);
                firstRow = table.First("tbody tr");
                firstRow.First("td").CheckIfInnerTextEquals("11");

                //page to back
                navigation = browser.ElementAt(".pagination", 0);
                navigation.FindElements("li a").Single(s => s.GetText() == "1").Click();
                browser.Wait(500);

                //after page back check edit row
                table = browser.ElementAt("table", tableID);
                firstRow = table.First("tbody tr");
                firstRow.First("td").CheckIfInnerTextEquals("1");
                firstRow.ElementAt("td", 1).Single("input").CheckIfIsDisplayed();
                firstRow.ElementAt("td", 2).Single("input").CheckIfIsDisplayed();
                firstRow.ElementAt("td", 3).FindElements("button").ThrowIfDifferentCountThan(2);


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

                Control_GridViewShowHeaderWhenNoData(browser);

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

        private void Control_GridViewShowHeaderWhenNoData(BrowserWrapper browser)
        {
            browser.FindElements("[data-ui='ShowHeaderWhenNoDataGrid']").FindElements("th").First().IsDisplayed();
        }

        [TestMethod]
        public void Control_GridViewRowDecorators()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewRowDecorators);
                browser.ElementAt("table", 0).FindElements("tr").ThrowIfDifferentCountThan(6);
                browser.ElementAt("table", 1).FindElements("tr").ThrowIfDifferentCountThan(6);

                // check that clicking selects the row which gets the 'selected' class
                // we dont want to check if element is clickable, it is not a button just fire click event
                browser.ElementAt("tr", 3).ElementAt("td", 0).Click();
                browser.Wait();
                for (int i = 0; i < 6; i++)
                {
                    browser.ElementAt("table", 0).ElementAt("tr", i).CheckClassAttribute(v => v.Contains("selected") == (i == 3));
                }
                // we dont want to check if element is clickable, it is not a button just fire click event
                browser.ElementAt("tr", 2).ElementAt("td", 0).Click();
                browser.Wait();
                for (int i = 0; i < 6; i++)
                {
                    browser.ElementAt("table", 0).ElementAt("tr", i).CheckClassAttribute(v => v.Contains("selected") == (i == 2));
                }
                
                // check that the edit row has the 'edit' class while the other rows have the 'normal' class
                for (int i = 1; i < 6; i++)
                {
                    if (i != 2)
                    {
                        browser.ElementAt("table", 1).ElementAt("tr", i).CheckIfHasClass("normal").CheckIfHasNotClass("edit");
                    }
                    else
                    {
                        browser.ElementAt("table", 1).ElementAt("tr", i).CheckIfHasClass("edit").CheckIfHasNotClass("normal");
                    }
                }
            });
        }

        [TestMethod]
        public void Control_GridViewRowDecorators_ClickPropagation()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewRowDecorators);
                browser.Wait();

                browser.ElementAt("table", 0).ElementAt("tr", 4).First("input[type=button]").Click().Wait();
                browser.ElementAt("table", 0).ElementAt("tr", 4).CheckIfHasNotClass("selected");
                browser.ElementAt("table", 0).ElementAt("tr", 4).ElementAt("td", 1).CheckIfInnerText(t => t == "xxx");

                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewRowDecorators);
                browser.Wait();

                browser.ElementAt("table", 0).ElementAt("tr", 4).First("a").Click().Wait();
                browser.ElementAt("table", 0).ElementAt("tr", 4).CheckIfHasNotClass("selected");
                browser.ElementAt("table", 0).ElementAt("tr", 4).ElementAt("td", 1).CheckIfInnerText(t => t == "xxx");
            });
        }

        [TestMethod]
        public void Control_GridViewColumnVisible()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_ColumnVisible);
                browser.Wait();

                // check that columns are visible
                for (int i = 0; i < 6; i++)
                {
                    browser.ElementAt("table", 0).ElementAt("tr", i).ElementAt("td,th", 2).CheckIfIsDisplayed();
                }
                for (int i = 0; i < 2; i++)
                {
                    browser.ElementAt("table", 1).ElementAt("tr", i).ElementAt("td,th", 1).CheckIfIsDisplayed();
                }

                // check that columns are hidden
                browser.First("input[type=checkbox]").Click();
                for (int i = 0; i < 6; i++)
                {
                    browser.ElementAt("table", 0).ElementAt("tr", i).ElementAt("td,th", 2).CheckIfIsNotDisplayed();
                }
                for (int i = 0; i < 2; i++)
                {
                    browser.ElementAt("table", 1).ElementAt("tr", i).ElementAt("td,th", 1).CheckIfIsNotDisplayed();
                }

                // check that columns are visible again
                browser.First("input[type=checkbox]").Click();
                for (int i = 0; i < 6; i++)
                {
                    browser.ElementAt("table", 0).ElementAt("tr", i).ElementAt("td,th", 2).CheckIfIsDisplayed();
                }
                for (int i = 0; i < 2; i++)
                {
                    browser.ElementAt("table", 1).ElementAt("tr", i).ElementAt("td,th", 1).CheckIfIsDisplayed();
                }
            });
        }

    }
}
