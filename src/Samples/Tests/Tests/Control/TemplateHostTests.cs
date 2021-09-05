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
    public class TemplateHostTests : AppSeleniumTest
    {
        public TemplateHostTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Control_TemplateHost_Basic()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_TemplateHost_Basic);

                AssertUI.TextEquals(browser.Single("fieldset legend"), "Form 1");
                AssertUI.TextEquals(browser.Single("fieldset p"), "hello from template");

                var items = browser.FindElements(".templated-list div");
                items.ThrowIfDifferentCountThan(3);

                // increment item
                AssertUI.TextEquals(items[0].Single("big"), "1");
                items[0].ElementAt("input[type=button]", 1).Click();
                AssertUI.TextEquals(items[0].Single("big"), "0");

                // remove item
                items[0].Single("a").Click();
                browser.WaitFor(() => {
                    items = browser.FindElements(".templated-list div");
                    items.ThrowIfDifferentCountThan(2);
                }, 2000);
                AssertUI.TextEquals(items[0].Single("big"), "2");

                // add item
                browser.Last("input[type=button]").Click();
                browser.WaitFor(() => {
                    items = browser.FindElements(".templated-list div");
                    items.ThrowIfDifferentCountThan(3);
                }, 2000);
            });
        }
    }
}
