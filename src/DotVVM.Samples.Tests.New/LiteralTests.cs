using DotVVM.Testing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Assert = Riganti.Utils.Testing.Selenium.Core.Assert;

namespace DotVVM.Samples.Tests.New
{
    public class LiteralTests : AppSeleniumTest
    {
        public LiteralTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Control_Literal_Literal()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Literal_Literal);

                foreach (var column in browser.FindElements("td"))
                {
                    Assert.InnerTextEquals(column.ElementAt("fieldset", 0).Single("span"), "Hardcoded value");
                    Assert.InnerTextEquals(column.ElementAt("fieldset", 1).Single("span"), "Hello");
                    Assert.InnerTextEquals(column.ElementAt("fieldset", 2).Single("span"), "1/1/2000");

                    Assert.NotContainsElement(column.ElementAt("fieldset", 3), "span");
                    Assert.InnerText(column.ElementAt("fieldset", 3), text => text.Contains("Hardcoded value"));

                    Assert.NotContainsElement(column.ElementAt("fieldset", 4), "span");
                    Assert.InnerText(column.ElementAt("fieldset", 4), text => text.Contains("Hello"));

                    Assert.NotContainsElement(column.ElementAt("fieldset", 5), "span");
                    Assert.InnerText(column.ElementAt("fieldset", 5), text => text.Contains("1/1/2000"));
                }
            });
        }

        [Fact]
        public void Control_Literal_Literal_FormatString()
        {
            RunInAllBrowsers(browser =>
            {
                Action<string> checkFormat = (string format) =>
                {
                    //check format d
                    var text = browser.First("#results-" + format).GetText();
                    browser.First("#client-format-" + format).CheckIfInnerTextEquals(text, false);
                    Assert.InnerTextEquals(browser.First("#client-format-" + format), text, false);
                    Assert.InnerText(browser.First("#client-format-" + format), s => s.Equals(text, StringComparison.OrdinalIgnoreCase));
                };

                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Literal_Literal_FormatString);
                browser.First("#change-culture").Click();

                checkFormat("d");
                checkFormat("D");
                //dd
            });
        }

        [Fact]
        public void Control_Literal_Literal_CollectionLength()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Literal_Literal_CollectionLength);

                Assert.InnerText(browser.Single("span"), s => s.Contains("0"));
                Assert.IsNotDisplayed(browser.Single("#second"));
                browser.First("#first").Click();

                Assert.InnerText(browser.Single("span"), s => s.Contains("1"));
                Assert.IsNotDisplayed(browser.Single("#second"));
                browser.First("#first").Click();

                Assert.InnerText(browser.Single("span"), s => s.Contains("2"));
                Assert.IsNotDisplayed(browser.Single("#second"));
                browser.First("#first").Click();

                Assert.InnerText(browser.Single("span"), s => s.Contains("3"));
                Assert.IsDisplayed(browser.Single("#second"));

            });
        }

        [Fact]
        public void Control_Literal_Literal_ArrayLength()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Literal_Literal_ArrayLength);


                Assert.InnerText(browser.Single("span"), s => s.Contains("0"));
                Assert.IsNotDisplayed(browser.Single("#second"));
                browser.First("#first").Click();

                Assert.InnerText(browser.Single("span"), s => s.Contains("1"));
                Assert.IsNotDisplayed(browser.Single("#second"));
                browser.First("#first").Click();

                Assert.InnerText(browser.Single("span"), s => s.Contains("2"));
                Assert.IsNotDisplayed(browser.Single("#second"));
                browser.First("#first").Click();

                Assert.InnerText(browser.Single("span"), s => s.Contains("3"));
                Assert.IsDisplayed(browser.Single("#second"));
            });
        }
    }
}
