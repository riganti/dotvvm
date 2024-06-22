using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class DataSetTests : AppSeleniumTest
    {
        public DataSetTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(SamplesRouteUrls.FeatureSamples_DataSet_GitHubApi)]
        [InlineData(SamplesRouteUrls.FeatureSamples_DataSet_GitHubApiStaticCommands)]
        public void Feature_DataSet_GitHubApi_Next(string url)
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(url);

                var grid = browser.Single("next-grid", SelectByDataUi);
                var pager = browser.Single("next-pager", SelectByDataUi);

                // get first issue on the first page
                var issueId = grid.ElementAt("tbody tr td", 0).GetInnerText();

                // go next
                pager.ElementAt("li", 1).Single("a").Click().Wait(500);
                AssertUI.TextNotEquals(grid.ElementAt("tbody tr td", 0), issueId);

                // go to first page
                pager.ElementAt("li", 0).Single("a").Click().Wait(500);
                AssertUI.TextEquals(grid.ElementAt("tbody tr td", 0), issueId);
            });
        }

        [Theory]
        [InlineData(SamplesRouteUrls.FeatureSamples_DataSet_GitHubApi)]
        [InlineData(SamplesRouteUrls.FeatureSamples_DataSet_GitHubApiStaticCommands)]
        public void Feature_DataSet_GitHubApi_NextHistory(string url)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(url);

                var grid = browser.Single("next-history-grid", SelectByDataUi);
                var pager = browser.Single("next-history-pager", SelectByDataUi);

                // get first issue on the first page
                var issueId1 = grid.ElementAt("tbody tr td", 0).GetInnerText();

                // go to page 2
                pager.ElementAt("li", 3).Single("a").Click().Wait(500);
                AssertUI.TextNotEquals(grid.ElementAt("tbody tr td", 0), issueId1);
                var issueId2 = grid.ElementAt("tbody tr td", 0).GetInnerText();

                // go to next page
                pager.ElementAt("li", 5).Single("a").Click().Wait(500);
                AssertUI.TextNotEquals(grid.ElementAt("tbody tr td", 0), issueId1);
                AssertUI.TextNotEquals(grid.ElementAt("tbody tr td", 0), issueId2);
                var issueId3 = grid.ElementAt("tbody tr td", 0).GetInnerText();

                // go to first page
                pager.ElementAt("li", 0).Single("a").Click().Wait(500);
                AssertUI.TextEquals(grid.ElementAt("tbody tr td", 0), issueId1);

                // go to page 4
                pager.ElementAt("li", 5).Single("a").Click().Wait(500);
                AssertUI.TextNotEquals(grid.ElementAt("tbody tr td", 0), issueId1);
                AssertUI.TextNotEquals(grid.ElementAt("tbody tr td", 0), issueId2);
                AssertUI.TextNotEquals(grid.ElementAt("tbody tr td", 0), issueId3);

                // go to previous page
                pager.ElementAt("li", 1).Single("a").Click().Wait(500);
                AssertUI.TextEquals(grid.ElementAt("tbody tr td", 0), issueId3);
            });
        }
    }
}
