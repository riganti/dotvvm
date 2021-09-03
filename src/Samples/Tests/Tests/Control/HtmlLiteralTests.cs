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
    public class HtmlLiteralTests : AppSeleniumTest
    {
        [Fact]
        public void Control_HtmlLiteral_HtmlLiteral()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_HtmlLiteral_HtmlLiteral);

                var column1 = browser.ElementAt("td", 0);
                var column2 = browser.ElementAt("td", 1);

                AssertUI.InnerTextEquals(column1.ElementAt("fieldset", 0).Single("div"), "Hello value");

                AssertUI.InnerTextEquals(column2.ElementAt("fieldset", 0).Single("div"), "Hello value");

                column2.ElementAt("fieldset", 1).FindElements("div").ThrowIfDifferentCountThan(0);
                AssertUI.InnerText(column2.ElementAt("fieldset", 1), t => t.Contains("Hello value"));
            });
        }

        public HtmlLiteralTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
