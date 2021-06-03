using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.Core.Api;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class ApiTests : AppSeleniumTest
    {
        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_Api_GithubRepoApi))]
        public void Feature_Api_GithubRepoApi()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Api_GithubRepoApi);

                IEnumerable<IElementWrapper> options = null;
                browser.WaitFor(() => {
                    options = browser.First("select").FindElements("option");
                    return options.Any(o => o.GetInnerText() == "dotvvm");
                }, 10000);

                // check dotvvm repo issues
                browser.Wait(2000);
                var dotvvmIssues = browser.First("table").FindElements("tr").Skip(1).ToList();
                Assert.True(dotvvmIssues.Count > 10);

                // get text of the first issue
                dotvvmIssues.ElementAt(0).First("a").Click();
                browser.Wait(2000);
                var firstIssueText = browser.First(".id-current-issue-text").GetInnerText();

                // make sure it changes when I click another issue
                dotvvmIssues.ElementAt(dotvvmIssues.Count - 1).First("a").Click();
                browser.Wait(2000);
                var lastIssueText = browser.First(".id-current-issue-text").GetInnerText();

                Assert.NotEqual(firstIssueText, lastIssueText);

                // switch to DotVVM Docs
                Assert.Contains(options, o => o.GetInnerText() == "dotvvm-docs");
                browser.First("select").Select("dotvvm-docs");
                browser.Wait(2000);

                // make sure that the table has changed
                var docsIssues = browser.First("table").FindElements("tr").Skip(1).ToList();
                Assert.True(docsIssues.Count > 1);

                docsIssues.ElementAt(0).First("a").Click();
                browser.Wait(2000);
                var firstIssueText2 = browser.First(".id-current-issue-text").GetInnerText();
                Assert.NotEqual(firstIssueText, firstIssueText2);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_Api_GetCollection))]
        public void Feature_Api_GetCollection()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Api_GetCollection);

                // click the first button (ID = 11)
                browser.WaitFor(() => {
                    browser.First(".id-company[data-company-id='11'] input[type=button]").Click()
                        .Wait();
                }, 30000, "Cannot find CompanyID = 11. Probably data are not loaded. (The page did not load in 5s.)");

                // ensure that orders have been loaded
                var orders = browser.FindElements(".id-order");
                AssertUI.Any(orders).Attribute("data-order-id", "6");

                var idToDelete = orders[2].GetAttribute("data-order-id");       // every order has two elements (read-only and edit)

                // delete order (ID = 7)
                browser.First($".id-order[data-order-id='{idToDelete}'] input[type=button][value=Delete]").Click().Wait();
                orders = browser.FindElements(".id-order");
                AssertUI.Any(orders).Attribute("data-order-id", "6");
                AssertUI.All(orders).Attribute("data-order-id", s => s != idToDelete);

                // click the second button (ID = 12)
                browser.First(".id-company[data-company-id='12'] input[type=button]").Click().Wait();

                // ensure that orders have been loaded
                orders = browser.FindElements(".id-order");
                AssertUI.Any(orders).Attribute("data-order-id", "2");
                AssertUI.Any(orders).Attribute("data-order-id", "9");

                // edit order (ID = 2)
                browser.First(".id-order[data-order-id='2'] input[type=button][value=Edit]").Click().Wait();
                browser.First(".id-order.id-edit input[type=text]").Clear().SendKeys("2000-01-01");
                browser.First(".id-order.id-edit input[type=button][value=Apply]").Click().Wait();
                browser.First(".id-order.id-edit input[type=button][value=Exit]").Click().Wait();

                AssertUI.TextEquals(browser.First(".id-order[data-order-id='2'] .id-date"), "2000-01-01");

                // change the order (ID = 2) date back so the test can be run once again
                browser.First(".id-order[data-order-id='2'] input[type=button][value=Edit]").Click().Wait();
                browser.First(".id-order.id-edit input[type=text]").Clear().SendKeys("2010-01-01");
                browser.First(".id-order.id-edit input[type=button][value=Apply]").Click().Wait();
                browser.First(".id-order.id-edit input[type=button][value=Exit]").Click().Wait();

                AssertUI.TextEquals(browser.First(".id-order[data-order-id='2'] .id-date"), "2010-01-01");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_Api_AzureFunctionsApi))]
        public void Feature_Api_AzureFunctionsApi()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Api_AzureFunctionsApi);
                string originalDate1 = null;
                string refreshedDate1 = null;

                browser.WaitFor(() => {
                    var date1 = browser.First(".id-date1");
                    AssertUI.TextNotEmpty(date1);
                    originalDate1 = date1.GetText();
                }, 15000, "Page did not loaded in 15s.");

                // click the get data button
                browser.First("input[type=button]").Click();

                browser.WaitFor(() => {
                    var date1 = browser.First(".id-date1");
                    AssertUI.TextNotEquals(date1, originalDate1);
                    refreshedDate1 = date1.GetText();
                }, 10000, "#LI :1");

                // test again
                originalDate1 = refreshedDate1;
                browser.Wait(1500);

                // click it again - the time changes every second

                browser.First("input[type=button]").Click();
                browser.WaitFor(() => {
                    var date1 = browser.First(".id-date1");
                    AssertUI.TextNotEquals(date1, originalDate1);
                    refreshedDate1 = date1.GetText();
                }, 10000, "#LI :2");

                // click the set data button
                browser.ElementAt("input[type=button]", 1).Click();

                browser.WaitFor(() => {
                    var date2 = browser.First(".id-date2");
                    AssertUI.TextEquals(date2, refreshedDate1);
                }, 10000);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_Api_AzureFunctionsApiTable))]
        public void Feature_Api_AzureFunctionsApiTable()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Api_AzureFunctionsApiTable);
                browser.WaitUntilDotvvmInited();
                var uiTestName = Guid.NewGuid().ToString();

                // fill Add entity form
                browser.ElementAt(".form-create input[type=text]", 0).Clear().SendKeys(uiTestName);
                browser.ElementAt(".form-create input[type=text]", 1).Clear().SendKeys("15");
                browser.ElementAt(".form-create input[type=text]", 2).Clear().SendKeys("2018-10-28 12:13:14");

                // submit
                browser.ElementAt(".form-create input[type=button]", 0).Click();
                browser.WaitForPostback(8000);

                browser.ElementAt(".form-create input[type=button]", 1).Click();
                browser.WaitForPostback(8000);

                // make sure the new row is in the table
                browser.WaitFor(() => {

                    var row = browser.FindElements(".form-grid tr").Skip(1).First(r => r.ElementAt("td", 0).GetText() == uiTestName);
                    AssertUI.TextEquals(row.ElementAt("td", 1), "15");
                    AssertUI.TextEquals(row.ElementAt("td", 2), "2018-10-28 12:13:14");
                }, 8000);

                // delete UI Test items
                foreach (var r in browser.FindElements(".form-grid tr").Skip(1).Where(r => r.ElementAt("td", 0).GetText() == uiTestName))
                {
                    r.First("input[type=checkbox]").Click();
                }
                browser.First(".form-grid input[type=button]").Click().Wait();
                browser.ElementAt(".form-create input[type=button]", 1).Click();

                // make sure it disappeared
                browser.WaitFor(() => {
                    Assert.Equal(0, browser.FindElements(".form-grid tr").Skip(1).Count(r => r.ElementAt("td", 0).GetText() == uiTestName));
                }, 8000);
            });
        }

        [Fact]
        public void Feature_Api_BindingSharing()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Api_BindingSharing);

                // wait for the page is loaded
                browser.WaitFor(() => {
                    for (int i = 0; i < 6; i++)
                    {
                        browser.ElementAt("select", 0).FindElements("option").ThrowIfSequenceEmpty();
                    }
                }, 10000, "The ComboBoxes didn't load.");

                // check combobox contents
                var combos = browser.FindElements("select");
                combos.ThrowIfDifferentCountThan(6);

                AssertUI.TextEquals(combos[0].ElementAt("option", 0), "Category 1 / Item 1");
                AssertUI.TextEquals(combos[0].ElementAt("option", 1), "Category 1 / Item 2");
                AssertUI.TextEquals(combos[0].ElementAt("option", 2), "Category 1 / Item 3");
                AssertUI.TextEquals(combos[1].ElementAt("option", 0), "Category 2 / Item 1");
                AssertUI.TextEquals(combos[1].ElementAt("option", 1), "Category 2 / Item 2");
                AssertUI.TextEquals(combos[1].ElementAt("option", 2), "Category 2 / Item 3");
                AssertUI.TextEquals(combos[1].ElementAt("option", 3), "Category 2 / Item 4");
                AssertUI.TextEquals(combos[1].ElementAt("option", 4), "Category 2 / Item 5");
                AssertUI.TextEquals(combos[2].ElementAt("option", 0), "Category 3 / Item 1");

                AssertUI.TextEquals(combos[3].ElementAt("option", 0), "Category 1 / Item 1");
                AssertUI.TextEquals(combos[3].ElementAt("option", 1), "Category 1 / Item 2");
                AssertUI.TextEquals(combos[3].ElementAt("option", 2), "Category 1 / Item 3");
                AssertUI.TextEquals(combos[4].ElementAt("option", 0), "Category 2 / Item 1");
                AssertUI.TextEquals(combos[4].ElementAt("option", 1), "Category 2 / Item 2");
                AssertUI.TextEquals(combos[4].ElementAt("option", 2), "Category 2 / Item 3");
                AssertUI.TextEquals(combos[4].ElementAt("option", 3), "Category 2 / Item 4");
                AssertUI.TextEquals(combos[4].ElementAt("option", 4), "Category 2 / Item 5");
                AssertUI.TextEquals(combos[5].ElementAt("option", 0), "Category 3 / Item 1");

                browser.Wait(1000);

                // check requests
                var requests = browser.Single("pre").GetInnerText().Split('\r', '\n').Where(l => l.Trim().Length > 0).ToList();
                Assert.Single(requests, r => r.EndsWith("BindingSharing/get?category=1"));
                Assert.Single(requests, r => r.EndsWith("BindingSharing/get?category=2"));
                Assert.Single(requests, r => r.EndsWith("BindingSharing/get?category=3"));
                Assert.Single(requests, r => r.EndsWith("BindingSharing/getWithRouteParam/1"));
                Assert.Single(requests, r => r.EndsWith("BindingSharing/getWithRouteParam/2"));
                Assert.Single(requests, r => r.EndsWith("BindingSharing/getWithRouteParam/3"));

                // click on the button
                browser.Single("input[type=button]").Click();
                browser.Wait(2000);

                combos = browser.FindElements("select");
                combos.ThrowIfDifferentCountThan(9);

                AssertUI.TextEquals(combos[6].ElementAt("option", 0), "Category 1 / Item 1");
                AssertUI.TextEquals(combos[6].ElementAt("option", 1), "Category 1 / Item 2");
                AssertUI.TextEquals(combos[6].ElementAt("option", 2), "Category 1 / Item 3");
                AssertUI.TextEquals(combos[7].ElementAt("option", 0), "Category 2 / Item 1");
                AssertUI.TextEquals(combos[7].ElementAt("option", 1), "Category 2 / Item 2");
                AssertUI.TextEquals(combos[7].ElementAt("option", 2), "Category 2 / Item 3");
                AssertUI.TextEquals(combos[7].ElementAt("option", 3), "Category 2 / Item 4");
                AssertUI.TextEquals(combos[7].ElementAt("option", 4), "Category 2 / Item 5");
                AssertUI.TextEquals(combos[8].ElementAt("option", 0), "Category 3 / Item 1");

                // check requests
                requests = browser.Single("pre").GetInnerText().Split('\r', '\n').Where(l => l.Trim().Length > 0).ToList();
                Assert.Equal(2, requests.Count(r => r.EndsWith("BindingSharing/get?category=1")));
                Assert.Equal(2, requests.Count(r => r.EndsWith("BindingSharing/get?category=2")));
                Assert.Equal(2, requests.Count(r => r.EndsWith("BindingSharing/get?category=3")));
                Assert.Equal(2, requests.Count(r => r.EndsWith("BindingSharing/getWithRouteParam/1")));
                Assert.Equal(2, requests.Count(r => r.EndsWith("BindingSharing/getWithRouteParam/2")));
                Assert.Equal(2, requests.Count(r => r.EndsWith("BindingSharing/getWithRouteParam/3")));
                Assert.Single(requests, r => r.EndsWith("BindingSharing/post?category=1"));
                Assert.Single(requests, r => r.EndsWith("BindingSharing/post?category=2"));
                Assert.Single(requests, r => r.EndsWith("BindingSharing/post?category=3"));
            });
        }

        public ApiTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
