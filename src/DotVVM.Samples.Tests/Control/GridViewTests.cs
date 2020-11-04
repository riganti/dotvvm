using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class GridViewTests : AppSeleniumTest
    {
        public GridViewTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Control_GridView_GridViewInlineEditingValidation()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditingValidation);
                browser.Driver.Manage().Window.Maximize();

                //Get rows
                var rows = browser.First("table tbody");
                var firstRow = rows.ElementAt("tr", 0);

                //Edit
                firstRow.ElementAt("td", 5).First("button").Click();

                rows = browser.First("table tbody");
                firstRow = rows.ElementAt("tr", 0);

                //Check type
                AssertUI.Attribute(firstRow.ElementAt("td", 1).First("input"), "type", "text");
                AssertUI.Attribute(firstRow.ElementAt("td", 2).First("input"), "type", "text");
                AssertUI.Attribute(firstRow.ElementAt("td", 3).First("input"), "type", "text");
                AssertUI.Attribute(firstRow.ElementAt("td", 4).First("input"), "type", "text");

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

                AssertUI.InnerTextEquals(validationResult, "The Name field is required.");

                //change name
                firstRow.ElementAt("td", 1).First("input").SendKeys("Test");

                //clear email
                firstRow.ElementAt("td", 3).First("input").Clear();

                //update
                firstRow.ElementAt("td", 5).First("button").Click();

                //check validation
                AssertUI.InnerTextEquals(validationResult, "The Email field is not a valid e-mail address.");
            });
        }

        [Fact]
        public void Control_GridView_GridViewStaticCommand()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewStaticCommand);
                browser.Wait();

                //check rows
                browser.FindElements("table tbody tr").ThrowIfDifferentCountThan(5);
                //check first row Id
                AssertUI.InnerTextEquals(browser.First("table tbody tr td span"), "1");
                //cal static command for delete row
                browser.First("table tbody tr input[type=button]").Click();
                //check rows again
                browser.FindElements("table tbody tr").ThrowIfDifferentCountThan(4);
                //check first row Id
                AssertUI.InnerTextEquals(browser.First("table tbody tr td span"), "2");
            });
        }

        [Fact]
        public void Control_GridView_GridViewInlineEditingValidation_GridViewInlineEditingFormat()
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
                AssertUI.TextEquals(firstRow.ElementAt("td", 2).First("input"), dateDisplay);
                AssertUI.TextEquals(firstRow.ElementAt("td", 4).First("input"), moneyDisplay);
            });
        }

        [Fact]
        public void Control_GridView_GridViewInlineEditingPrimaryKeyGuid()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditingPrimaryKeyGuid);
                //Get rows
                var rows = browser.First("table tbody");
                rows.FindElements("tr").ThrowIfDifferentCountThan(3);

                var firstRow = rows.ElementAt("tr", 0);
                AssertUI.InnerTextEquals(firstRow.ElementAt("td", 0).First("span"), "9536d712-2e91-43d2-8ebb-93fbec31cf34");
                //Edit
                firstRow.ElementAt("td", 4).First("button").Click();
                browser.Wait(500);

                //init again
                rows = browser.First("table tbody");
                firstRow = rows.ElementAt("tr", 0);

                //Check type
                AssertUI.Attribute(firstRow.ElementAt("td", 1).First("input"), "type", "text");
                AssertUI.Attribute(firstRow.ElementAt("td", 2).First("input"), "type", "text");
                AssertUI.Attribute(firstRow.ElementAt("td", 3).First("input"), "type", "text");

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
                AssertUI.InnerTextEquals(firstRow.ElementAt("td", 1).First("span"), "Test");
            });
        }

        [Fact]
        public void Control_GridView_GridViewInlineEditingPrimaryKeyString()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditingPrimaryKeyString);
                //Get rows
                var rows = browser.First("table tbody");
                rows.FindElements("tr").ThrowIfDifferentCountThan(3);

                var firstRow = rows.ElementAt("tr", 0);
                AssertUI.InnerTextEquals(firstRow.ElementAt("td", 0).First("span"), "A");
                //Edit
                firstRow.ElementAt("td", 4).First("button").Click();
                browser.Wait(500);

                //init again
                rows = browser.First("table tbody");
                firstRow = rows.ElementAt("tr", 0);

                //Check type
                AssertUI.Attribute(firstRow.ElementAt("td", 1).First("input"), "type", "text");
                AssertUI.Attribute(firstRow.ElementAt("td", 2).First("input"), "type", "text");
                AssertUI.Attribute(firstRow.ElementAt("td", 3).First("input"), "type", "text");

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
                AssertUI.InnerTextEquals(firstRow.ElementAt("td", 0).First("span"), "A");
                AssertUI.InnerTextEquals(firstRow.ElementAt("td", 1).First("span"), "Test");
            });
        }

        [Theory]
        [InlineData(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditing, 0)]
        [InlineData(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditing, 1)]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditing))]
        public void Control_GridView_GridViewInlineEditing(string path, int tableID)
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
                AssertUI.InnerTextEquals(firstRow.First("td"), "1");
                AssertUI.IsDisplayed(firstRow.ElementAt("td", 1).Single("input"));
                AssertUI.IsDisplayed(firstRow.ElementAt("td", 2).Single("input"));
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
                AssertUI.IsDisplayed(desiredRow.First("input"));
                desiredRow.FindElements("button").ThrowIfDifferentCountThan(2);
            });
        }

        [Theory]
        [InlineData(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditing, 0)]
        [InlineData(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditing, 1)]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_GridView_GridViewInlineEditing))]
        public void Control_GridView_GridViewInlineEditing_PagingWhenEditing(string path, int tableID)
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
                AssertUI.InnerTextEquals(firstRow.First("td"), "1");
                AssertUI.IsDisplayed(firstRow.ElementAt("td", 1).Single("input"));
                AssertUI.IsDisplayed(firstRow.ElementAt("td", 2).Single("input"));
                firstRow.ElementAt("td", 3).FindElements("button").ThrowIfDifferentCountThan(2);

                // check if right number of testboxs are displayed => IsEditable works
                table.FindElements("tbody tr td input").ThrowIfDifferentCountThan(2);

                //page to second page
                var navigation = browser.ElementAt(".pagination", 0);
                navigation.FindElements("li a").Single(s => s.GetText() == "2").Click();
                browser.Wait(500);

                table = browser.ElementAt("table", tableID);
                firstRow = table.First("tbody tr");
                AssertUI.InnerTextEquals(firstRow.First("td"), "11");

                //page to back
                navigation = browser.ElementAt(".pagination", 0);
                navigation.FindElements("li a").Single(s => s.GetText() == "1").Click();
                browser.Wait(500);

                //after page back check edit row
                table = browser.ElementAt("table", tableID);
                firstRow = table.First("tbody tr");
                AssertUI.InnerTextEquals(firstRow.First("td"), "1");
                AssertUI.IsDisplayed(firstRow.ElementAt("td", 1).Single("input"));
                AssertUI.IsDisplayed(firstRow.ElementAt("td", 2).Single("input"));
                firstRow.ElementAt("td", 3).FindElements("button").ThrowIfDifferentCountThan(2);
            });
        }

        [Theory]
        [InlineData(SamplesRouteUrls.ControlSamples_GridView_GridViewPagingSorting)]
        [InlineData(SamplesRouteUrls.ControlSamples_GridView_GridViewServerRender)]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_GridView_GridViewPagingSorting))]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_GridView_GridViewServerRender))]
        public void Control_GridView_GridViewPagingSortingBase(string path)
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(path);
                browser.ActionWaitTime = 500;

                System.Action performTest = () =>
                {
                    //// make sure that thirs row's first cell is yellow
                    AssertUI.ClassAttribute(browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0), s => s.Equals(""));
                    AssertUI.ClassAttribute(browser.ElementAt("table", 0).ElementAt("tr", 2).ElementAt("td", 0), s => s.Equals("alternate"));

                    //// go to second page
                    AssertUI.InnerTextEquals(browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0), "1");
                    browser.ElementAt("ul", 0).FindElements("li a").Single(s => s.GetText() == "2").Click();
                    browser.Wait();

                    //// go to previous page
                    AssertUI.InnerTextEquals(browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0), "11");
                    browser.ElementAt("ul", 0).FindElements("li a").Single(s => s.GetText() == "««").Click();
                    browser.Wait();

                    //// go to next page
                    AssertUI.InnerTextEquals(browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0), "1");
                    browser.ElementAt("ul", 0).FindElements("li a").Single(s => s.GetText() == "»»").Click();
                    browser.Wait();

                    //// try the disabled link - nothing should happen
                    AssertUI.InnerTextEquals(browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0), "11");
                    browser.ElementAt("ul", 0).FindElements("li a").Single(s => s.GetText() == "»»").Click();
                    browser.Wait();

                    AssertUI.InnerTextEquals(browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0), "11");

                    // try sorting in the first grid
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 2).ElementAt("button", 0).Click();
                    browser.Wait();
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 1).ElementAt("a", 0).Click();
                    browser.Wait();
                    AssertUI.ClassAttribute(browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 1), "sort-asc");

                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 0).ElementAt("a", 0).Click();
                    browser.Wait();
                    AssertUI.InnerTextEquals(browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0), "1");

                    //// sort descending in the first grid
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 1).ElementAt("a", 0).Click();
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 1).ElementAt("a", 0).Click();
                    browser.Wait();
                    AssertUI.ClassAttribute(browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 1), "sort-desc");
                    AssertUI.InnerTextEquals(browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0), "16");

                    //// sort by different column in the first grid
                    browser.ElementAt("table", 0).ElementAt("tr", 0).ElementAt("th", 0).ElementAt("a", 0).Click();
                    browser.Wait();
                    AssertUI.InnerTextEquals(browser.ElementAt("table", 0).ElementAt("tr", 1).ElementAt("td", 0), "1");
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

        private void Control_GridViewShowHeaderWhenNoData(IBrowserWrapper browser)
        {
            browser.FindElements("[data-ui='ShowHeaderWhenNoDataGrid']").FindElements("th").First().IsDisplayed();
        }

        [Fact]
        public void Control_GridView_GridViewRowDecorators()
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
                    AssertUI.ClassAttribute(browser.ElementAt("table", 0).ElementAt("tr", i), v => v.Contains("selected") == (i == 3));
                }
                // we dont want to check if element is clickable, it is not a button just fire click event
                browser.ElementAt("tr", 2).ElementAt("td", 0).Click();
                browser.Wait();
                for (int i = 0; i < 6; i++)
                {
                    AssertUI.ClassAttribute(browser.ElementAt("table", 0).ElementAt("tr", i), v => v.Contains("selected") == (i == 2));
                }

                // check that the edit row has the 'edit' class while the other rows have the 'normal' class
                for (int i = 1; i < 6; i++)
                {
                    if (i != 2)
                    {
                        var elementWrapper = browser.ElementAt("table", 1).ElementAt("tr", i);
                        AssertUI.HasClass(elementWrapper, "normal");
                        AssertUI.HasNotClass(elementWrapper, "edit");
                    }
                    else
                    {
                        var elementWrapper = browser.ElementAt("table", 1).ElementAt("tr", i);
                        AssertUI.HasClass(elementWrapper, "edit");
                        AssertUI.HasNotClass(elementWrapper, "normal");
                    }
                }
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_GridView_GridViewRowDecorators))]
        public void Control_GridView_GridViewRowDecorators_ClickPropagation()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewRowDecorators);
                browser.Wait();

                browser.ElementAt("table", 0).ElementAt("tr", 4).First("input[type=button]").Click().Wait();
                AssertUI.HasNotClass(browser.ElementAt("table", 0).ElementAt("tr", 4), "selected");
                AssertUI.InnerText(browser.ElementAt("table", 0).ElementAt("tr", 4).ElementAt("td", 1), t => t == "xxx");

                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewRowDecorators);
                browser.Wait();

                browser.ElementAt("table", 0).ElementAt("tr", 4).First("a").Click().Wait();
                AssertUI.HasNotClass(browser.ElementAt("table", 0).ElementAt("tr", 4), "selected");
                AssertUI.InnerText(browser.ElementAt("table", 0).ElementAt("tr", 4).ElementAt("td", 1), t => t == "xxx");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_GridView_GridViewRowDecorators))]
        public void Control_GridView_GridViewRowDecorators_RouteLinkClickPropagation()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewRowDecorators);
                browser.Wait();

                var routeLinks = browser.FindElements("table [data-ui=route-link]");

                routeLinks.First().Click().Wait();
                AssertUI.UrlEquals(browser, browser.BaseUrl);
            });
        }

        [Fact]
        public void Control_GridView_ColumnVisible()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_ColumnVisible);
                browser.Wait();

                // check that columns are visible
                for (int i = 0; i < 6; i++)
                {
                    AssertUI.IsDisplayed(browser.ElementAt("table", 0).ElementAt("tr", i).ElementAt("td,th", 2));
                }
                for (int i = 0; i < 2; i++)
                {
                    AssertUI.IsDisplayed(browser.ElementAt("table", 1).ElementAt("tr", i).ElementAt("td,th", 1));
                }

                // check that columns are hidden
                browser.First("input[type=checkbox]").Click();
                for (int i = 0; i < 6; i++)
                {
                    AssertUI.IsNotDisplayed(browser.ElementAt("table", 0).ElementAt("tr", i).ElementAt("td,th", 2));
                }
                for (int i = 0; i < 2; i++)
                {
                    AssertUI.IsNotDisplayed(browser.ElementAt("table", 1).ElementAt("tr", i).ElementAt("td,th", 1));
                }

                // check that columns are visible again
                browser.First("input[type=checkbox]").Click();
                for (int i = 0; i < 6; i++)
                {
                    AssertUI.IsDisplayed(browser.ElementAt("table", 0).ElementAt("tr", i).ElementAt("td,th", 2));
                }
                for (int i = 0; i < 2; i++)
                {
                    AssertUI.IsDisplayed(browser.ElementAt("table", 1).ElementAt("tr", i).ElementAt("td,th", 1));
                }
            });
        }

        [Fact]
        public void Control_GridView_LargeGrid()
        {
            const int RowCount = 1000;
            const int ColumnCount = 28;
            const string FirstCell = "FirstName0";
            const string LastCell = "DataZ999";
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_LargeGrid);

                // Check Row and Column count
                var tbody = browser.Single("tbody");
                tbody.FindElements("tr").ThrowIfDifferentCountThan(RowCount);
                tbody.First("tr").FindElements("td").ThrowIfDifferentCountThan(ColumnCount);

                // Check first cell
                AssertUI.TextEquals(tbody.First("tr").First("td").Single("span"), FirstCell);

                // Check last cell
                AssertUI.TextEquals(tbody.Last("tr").Last("td").Single("span"), LastCell);
            });
        }

        [Fact]
        public void Control_GridView_RenamedPrimaryKey()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_RenamedPrimaryKey);
                browser.WaitUntilDotvvmInited();

                var gridview = browser.Single("gridview", SelectByDataUi);
                AssertUI.NotContainsElement(gridview, "input");

                browser.First("edit-button", SelectByDataUi).Click();
                browser.WaitFor(() => AssertUI.ContainsElement(gridview, "input"), 1000);

                browser.First("save-button", SelectByDataUi).Click();
                browser.WaitFor(() => AssertUI.NotContainsElement(gridview, "input"), 1000);
            });
        }

        [Fact]
        public void Control_GridView_InvalidCssClass_TextBox_Attached()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_InvalidCssClass);
                browser.WaitUntilDotvvmInited();

                var gridview = browser.Single("gridview", SelectByDataUi);
                gridview.First("edit-button", SelectByDataUi).Click();

                IElementWrapper textbox = null;
                browser.WaitFor(() => textbox = browser.First(".name-attached > input"), 1000);
                AssertUI.HasNotClass(textbox, "invalid");
                textbox.Clear();

                gridview.First("save-button", SelectByDataUi).Click();
                browser.WaitFor(() => AssertUI.HasClass(textbox, "invalid"), 1000);
            });
        }

        [Fact]
        public void Control_GridView_InvalidCssClass_TextBox_Both()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_InvalidCssClass);
                browser.WaitUntilDotvvmInited();

                var gridview = browser.Single("gridview", SelectByDataUi);
                gridview.First("edit-button", SelectByDataUi).Click();

                IElementWrapper textbox = null;
                IElementWrapper validator = null;
                browser.WaitFor(() => textbox = browser.First(".name-attached-standalone > input"), 1000);
                browser.WaitFor(() => validator = browser.First(".name-attached-standalone > span"), 1000);
                AssertUI.HasNotClass(textbox, "invalid");
                AssertUI.HasNotClass(validator, "invalid");
                textbox.Clear();

                gridview.First("save-button", SelectByDataUi).Click();
                browser.WaitFor(() => AssertUI.HasClass(textbox, "invalid"), 1000);
                browser.WaitFor(() => AssertUI.HasClass(validator, "invalid"), 1000);
            });
        }

        [Fact]
        public void Control_GridView_InvalidCssClass_CheckBox()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_InvalidCssClass);
                browser.WaitUntilDotvvmInited();

                var gridview = browser.Single("gridview", SelectByDataUi);
                AssertUI.HasNotClass(gridview.First(".is-standalone > span"), "invalid");

                gridview.First("edit-button", SelectByDataUi).Click();
                var checkBox = browser.First(".is-standalone > input");
                checkBox.Click();
                gridview.First("save-button", SelectByDataUi).Click();
                browser.WaitFor(() => AssertUI.HasClass(gridview.First(".is-standalone > span"), "invalid"), 1000);
            });
        }

        [Fact]
        public void Control_GridView_GridViewSortChanged()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_GridViewSortChanged);
                browser.WaitUntilDotvvmInited();

                var tables = browser.FindElements("table");
                var sortExpression = browser.Single(".result-sortexpression");
                var sortDescending = browser.Single(".result-sortdescending");

                // click the Name column in the first table
                tables[0].ElementAt("th", 1).Single("a").Click().Wait();
                AssertUI.TextEquals(sortExpression, "Name");
                AssertUI.TextEquals(sortDescending, "false");

                // click the Name column in the first table again to change sort direction
                tables[0].ElementAt("th", 1).Single("a").Click().Wait();
                AssertUI.TextEquals(sortExpression, "Name");
                AssertUI.TextEquals(sortDescending, "true");

                // click the Message received column in the first table
                tables[0].ElementAt("th", 3).Single("a").Click().Wait();
                AssertUI.TextEquals(sortExpression, "MessageReceived");
                AssertUI.TextEquals(sortDescending, "false");

                // click the Message received column in the first table again to change sort direction
                tables[0].ElementAt("th", 3).Single("a").Click().Wait();
                AssertUI.TextEquals(sortExpression, "MessageReceived");
                AssertUI.TextEquals(sortDescending, "true");


                // click the Name column in the second table
                tables[1].ElementAt("th", 1).Single("a").Click().Wait();
                AssertUI.TextEquals(sortExpression, "Name");
                AssertUI.TextEquals(sortDescending, "false");

                // click the Name column in the second table again - sort direction should remain unchanged
                tables[1].ElementAt("th", 1).Single("a").Click().Wait();
                AssertUI.TextEquals(sortExpression, "Name");
                AssertUI.TextEquals(sortDescending, "false");

                // click the Message received column in the first table
                tables[1].ElementAt("th", 3).Single("a").Click().Wait();
                AssertUI.TextEquals(sortExpression, "MessageReceived");
                AssertUI.TextEquals(sortDescending, "false");

                // click the Message received column in the second table again - sort direction should remain unchanged
                tables[1].ElementAt("th", 3).Single("a").Click().Wait();
                AssertUI.TextEquals(sortExpression, "MessageReceived");
                AssertUI.TextEquals(sortDescending, "false");
            });
        }

        [Fact]
        public void Control_GridView_NestedGridViewsWithInlineEditing()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_GridView_NestedGridViewInlineEditing);
                browser.WaitUntilDotvvmInited();

                // Edit customer
                browser.FindElements("input[type=button]").First(b => b.GetText() == "Edit Customer").Click().Wait();
                // Edit customer name
                browser.First("input[type=text]").ClearInputByKeyboard().SendKeys("NewName");
                // Save customer
                browser.FindElements("input[type=button]").First(b => b.GetText() == "Save Customer").Click().Wait();

                // Edit shopping cart-item
                browser.FindElements("input[type=button]").First(b => b.GetText() == "Edit Cart-item").Click().Wait();
                // Edit quantity
                browser.First("input[type=text]").ClearInputByKeyboard().SendKeys("1111");
                // Save shooping cart-item
                browser.FindElements("input[type=button]").First(b => b.GetText() == "Save Cart-item").Click().Wait();
            });
        }
    }
}
