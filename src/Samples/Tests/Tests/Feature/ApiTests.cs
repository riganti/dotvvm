﻿using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.Core.Api;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;
using static DotVVM.Samples.Tests.UITestUtils;

namespace DotVVM.Samples.Tests.Feature
{
    public class ApiTests : AppSeleniumTest
    {
        [Fact]
        [Trait("Category", "owin-only")]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_Api_GetCollection))]
        public void Feature_Api_GetCollection()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Api_GetCollection);

                // click the first button (ID = 11)
                browser.WaitFor(() => {
                    browser.First(".id-company[data-company-id='11'] input[type=button]").Click();
                }, 30000, "Cannot find CompanyID = 11. Probably data are not loaded. (The page did not load in 5s.)");

                // ensure that orders have been loaded
                WaitForIgnoringStaleElements(() => {
                    AssertUI.Any(browser.FindElements(".id-order"), waitForOptions: WaitForOptions.Disabled).Attribute("data-order-id", "6");
                });

                var idToDelete = browser.FindElements(".id-order")[2].GetAttribute("data-order-id");       // every order has two elements (read-only and edit)

                // delete order (ID = 7)
                browser.First($".id-order[data-order-id='{idToDelete}'] input[type=button][value=Delete]").Click();
                WaitForIgnoringStaleElements(() => {
                    AssertUI.Any(browser.FindElements(".id-order"), WaitForOptions.Disabled).Attribute("data-order-id", "6");
                    AssertUI.All(browser.FindElements(".id-order"), WaitForOptions.Disabled).Attribute("data-order-id", s => s != idToDelete);
                });

                // click the second button (ID = 12)
                browser.First(".id-company[data-company-id='12'] input[type=button]").Click();


                // ensure that orders have been loaded
                WaitForIgnoringStaleElements(() => {
                    AssertUI.Any(browser.FindElements(".id-order"), WaitForOptions.Disabled).Attribute("data-order-id", "2");
                    AssertUI.Any(browser.FindElements(".id-order"), WaitForOptions.Disabled).Attribute("data-order-id", "9");
                });

                // edit order (ID = 2)
                browser.First(".id-order[data-order-id='2'] input[type=button][value=Edit]").Click();
                browser.First(".id-order.id-edit input[type=text]").Clear().SendKeys("2000-01-01");
                browser.First(".id-order.id-edit input[type=button][value=Apply]").Click();
                WaitForIgnoringStaleElements(() => {
                    browser.First(".id-order.id-edit input[type=button][value=Exit]").Click();
                });

                AssertUI.TextEquals(browser.First(".id-order[data-order-id='2'] .id-date"), "2000-01-01");

                // change the order (ID = 2) date back so the test can be run once again
                browser.First(".id-order[data-order-id='2'] input[type=button][value=Edit]").Click();
                browser.First(".id-order.id-edit input[type=text]").Clear().SendKeys("2010-01-01");
                browser.First(".id-order.id-edit input[type=button][value=Apply]").Click();
                WaitForIgnoringStaleElements(() => {
                    browser.First(".id-order.id-edit input[type=button][value=Exit]").Click();
                });

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
                browser.First(".form-grid input[type=button]").Click();
                browser.ElementAt(".form-create input[type=button]", 1).Click();

                // make sure it disappeared
                browser.WaitFor(() => {
                    Assert.Equal(0, browser.FindElements(".form-grid tr").Skip(1).Count(r => r.ElementAt("td", 0).GetText() == uiTestName));
                }, 8000);
            });
        }

        [Fact(Skip = "Doesn't work on OWIN because it relies on _apiCore.")]
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

        [Fact(Skip = "Doesn't work on OWIN because it relies on _apiCore.")]
        public void Feature_Api_ApiRefresh()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Api_ApiRefresh);
                
                browser.Single("not-updating", SelectByDataUi)
                    .FindElements("td")
                    .ThrowIfDifferentCountThan(0);
                browser.Single("updating", SelectByDataUi)
                    .FindElements("td")
                    .ThrowIfDifferentCountThan(0);
                AssertUI.TextEquals(browser.Single("number", SelectByDataUi), "1");

                browser.Single("input[type=text]").SendKeys(Keys.Backspace).SendKeys("11").SendKeys(Keys.Tab);

                browser.Single("not-updating", SelectByDataUi)
                    .FindElements("td")
                    .ThrowIfDifferentCountThan(0);
                browser.Single("updating", SelectByDataUi)
                    .FindElements("td")
                    .ThrowIfDifferentCountThan(10);

                AssertUI.TextEquals(browser.Single("number", SelectByDataUi), "2");

                browser.Single("input[type=text]").SendKeys(Keys.Backspace).SendKeys(Keys.Backspace).SendKeys("12").SendKeys(Keys.Tab);

                browser.Single("not-updating", SelectByDataUi)
                    .FindElements("td")
                    .ThrowIfDifferentCountThan(0);
                browser.Single("updating", SelectByDataUi)
                    .FindElements("td")
                    .ThrowIfDifferentCountThan(6);

                AssertUI.TextEquals(browser.Single("number", SelectByDataUi), "3");
            });
        }

        public ApiTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
