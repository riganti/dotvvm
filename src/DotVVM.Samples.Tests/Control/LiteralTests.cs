using System;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
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
                    AssertUI.InnerTextEquals(column.ElementAt("fieldset", 0).Single("span"), "Hardcoded value");
                    AssertUI.InnerTextEquals(column.ElementAt("fieldset", 1).Single("span"), "Hello");
                    AssertUI.InnerTextEquals(column.ElementAt("fieldset", 2).Single("span"), "1/1/2000");

                    AssertUI.NotContainsElement(column.ElementAt("fieldset", 3), "span");
                    AssertUI.InnerText(column.ElementAt("fieldset", 3), text => text.Contains("Hardcoded value"));

                    AssertUI.NotContainsElement(column.ElementAt("fieldset", 4), "span");
                    AssertUI.InnerText(column.ElementAt("fieldset", 4), text => text.Contains("Hello"));

                    AssertUI.NotContainsElement(column.ElementAt("fieldset", 5), "span");
                    AssertUI.InnerText(column.ElementAt("fieldset", 5), text => text.Contains("1/1/2000"));
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
                    AssertUI.InnerTextEquals(browser.First("#client-format-" + format), text, false);
                    AssertUI.InnerTextEquals(browser.First("#client-format-" + format), text, false);
                    AssertUI.InnerText(browser.First("#client-format-" + format), s => s.Equals(text, StringComparison.OrdinalIgnoreCase));
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

                AssertUI.InnerText(browser.Single("span"), s => s.Contains("0"));
                AssertUI.IsNotDisplayed(browser.Single("#second"));
                browser.First("#first").Click();

                AssertUI.InnerText(browser.Single("span"), s => s.Contains("1"));
                AssertUI.IsNotDisplayed(browser.Single("#second"));
                browser.First("#first").Click();

                AssertUI.InnerText(browser.Single("span"), s => s.Contains("2"));
                AssertUI.IsNotDisplayed(browser.Single("#second"));
                browser.First("#first").Click();

                AssertUI.InnerText(browser.Single("span"), s => s.Contains("3"));
                AssertUI.IsDisplayed(browser.Single("#second"));

            });
        }

        [Fact]
        public void Control_Literal_Literal_ArrayLength()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Literal_Literal_ArrayLength);


                AssertUI.InnerText(browser.Single("span"), s => s.Contains("0"));
                AssertUI.IsNotDisplayed(browser.Single("#second"));
                browser.First("#first").Click();

                AssertUI.InnerText(browser.Single("span"), s => s.Contains("1"));
                AssertUI.IsNotDisplayed(browser.Single("#second"));
                browser.First("#first").Click();

                AssertUI.InnerText(browser.Single("span"), s => s.Contains("2"));
                AssertUI.IsNotDisplayed(browser.Single("#second"));
                browser.First("#first").Click();

                AssertUI.InnerText(browser.Single("span"), s => s.Contains("3"));
                AssertUI.IsDisplayed(browser.Single("#second"));
            });
        }
    }
}
