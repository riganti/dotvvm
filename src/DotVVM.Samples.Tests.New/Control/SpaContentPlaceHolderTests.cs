using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.New.Control
{
    public class SpaContentPlaceHolderTests : AppSeleniumTest
    {
        public SpaContentPlaceHolderTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_HistoryApi_Default))]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_HistoryApi_PageA))]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_HistoryApi_PageB))]
        public void Control_SpaContentPlaceHolder_SpaContentPlaceHolder_HistoryApi()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_HistoryApi);
                browser.Wait(2000);

                // verify the URL after redirect to the DefaultRoute
                AssertUI.AlertTextContains(browser, "javascript 2 resource loaded!");
                browser.ConfirmAlert();
                browser.Wait(2000);
                AssertUI.AlertTextContains(browser, "javascript resource loaded!");
                browser.ConfirmAlert();
                AssertUI.UrlEquals(browser, browser.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_HistoryApi_PageB);

                // go to first page
                browser.First("a").Click();
                browser.Wait();
                AssertUI.UrlEquals(browser, browser.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_HistoryApi_PageA + "/16");

                // test first page
                AssertUI.TextEquals(browser.ElementAt("span", 0), "0");
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                AssertUI.TextEquals(browser.ElementAt("span", 0), "3");

                // go to second page
                browser.FindElements("a").Single(l => l.GetText() == "Go to Task List").Click();
                browser.Wait();
                AssertUI.UrlEquals(browser, browser.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_HistoryApi_PageB);

                // try the task list
                browser.FindElements(".table tr").ThrowIfDifferentCountThan(3);
                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                browser.Click("input[type=button]");
                browser.Wait();
                browser.FindElements(".table tr").ThrowIfDifferentCountThan(4);
                browser.Last("tr a").Click();
                browser.Wait();
                AssertUI.HasClass(browser.Last(".table tr"), "completed");

                // test the browse back button
                browser.NavigateBack();
                browser.Wait();
                AssertUI.UrlEquals(browser, browser.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_HistoryApi_PageA + "/16");

                // test first page
                AssertUI.TextEquals(browser.ElementAt("span", 0), "0");
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                AssertUI.TextEquals(browser.ElementAt("span", 0), "3");

                // test the forward button
                browser.NavigateForward();
                browser.Wait();

                // test the redirect inside SPA
                browser.FindElements(".table tr").ThrowIfDifferentCountThan(3);
                browser.Last("input[type=button]").Click();
                browser.Wait(2000);
                AssertUI.UrlEquals(browser, browser.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_HistoryApi_PageA + "/15");

                // test the redirect outside SPA
                AssertUI.TextEquals(browser.ElementAt("span", 0), "0");
                browser.FindElements("a").Single(l => l.GetText().Contains("Exit SPA")).Click();
                browser.Wait();
                AssertUI.UrlEquals(browser, browser.BaseUrl + SamplesRouteUrls.ComplexSamples_TaskList_ServerRenderedTaskList);
            });
        }
    }
}
