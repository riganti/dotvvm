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

namespace DotVVM.Samples.Tests.Control
{
    public class AppendableDataPagerTests : AppSeleniumTest
    {
        public AppendableDataPagerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Control_AppendableDataPager_AppendableDataPager()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_AppendableDataPager_AppendableDataPager);

                var table = browser.Single("table");
                table.FindElements("tbody tr").ThrowIfDifferentCountThan(3);

                // load more data
                for (var i = 1; i < 4; i++)
                {
                    browser.Single("input[type=button]").Click();
                    browser.WaitFor(() => {
                        browser.FindElements(".loading").ThrowIfDifferentCountThan(1, WaitForOptions.Disabled);
                    }, 2000);
                    table.FindElements("tbody tr").ThrowIfDifferentCountThan((i + 1) * 3);
                }

                // check we are at the end
                browser.FindElements("input[type=button]").ThrowIfDifferentCountThan(0);
                browser.FindElements(".loaded").ThrowIfDifferentCountThan(1);
            });
        }

        [Fact]
        public void Control_AppendableDataPager_AppendableDataPagerAutoLoad()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_AppendableDataPager_AppendableDataPagerAutoLoad);

                browser.FindElements(".customer h1").ThrowIfDifferentCountThan(3);
                browser.Wait(1000);

                // load more data by scrolling to the bottom
                for (var i = 1; i < 4; i++)
                {
                    browser.GetJavaScriptExecutor()
                        .ExecuteScript("window.scrollTo(0, document.body.scrollHeight - 10)");
                    browser.WaitFor(() => {
                        browser.FindElements(".loading").ThrowIfDifferentCountThan(1, WaitForOptions.Disabled);
                    }, 2000);
                    browser.FindElements(".customer h1").ThrowIfDifferentCountThan((i + 1) * 3);
                }

                // check we are at the end
                browser.FindElements(".loaded").ThrowIfDifferentCountThan(1);
            });
        }
    }
}
