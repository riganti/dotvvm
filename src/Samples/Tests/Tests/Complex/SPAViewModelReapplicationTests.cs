using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    public class SPAViewModelReapplicationTests : AppSeleniumTest
    {
        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPAViewModelReapplication_pageA))]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_SPAViewModelReapplication_pageB))]
        public void Complex_SPAViewModelReapplication()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_SPAViewModelReapplication_pageA);
                WaitForExecutor.WaitFor(() => {
                    // verify items count
                    browser.FindElements("ul#first li").ThrowIfDifferentCountThan(3, WaitForOptions.Disabled);
                });
                AssertUI.InnerText(browser.Single("#first"), s => s.Contains("Entry 1") && s.Contains("Entry 2") && s.Contains("Entry 3"));

                AssertUI.InnerTextEquals(browser.First("#test2"), "A");

                // verify first page values
                browser.First("input[type=text]").GetAttribute("value").Contains("Hello");
                browser.Last("input[type=text]").GetAttribute("value").Contains("1");

                //check url
                AssertUI.Url(browser, s => s.Contains("SPAViewModelReapplication/page"));

                // try the postback
                browser.First("input[type=button]").Click();
                AssertUI.InnerTextEquals(browser.First("#testResult"), "Hello1");

                // go to the second page
                browser.Single("#pageB").Click();

                // verify items count and
                browser.FindElements("ul#first li").ThrowIfDifferentCountThan(3);
                AssertUI.InnerText(browser.Single("#first"), s => s.Contains("Entry 1") && s.Contains("Entry 2") && s.Contains("Entry 3"));

                // verify second page values
                browser.First("input[type=text]").GetAttribute("value").Contains("World");
                browser.Last("input[type=text]").GetAttribute("value").Contains("2");
                AssertUI.InnerTextEquals(browser.First("#test2"), "B");

                // try the postback
                browser.First("input[type=button]").Click();
                AssertUI.InnerTextEquals(browser.First("#testResult"), "World2");

                // go to first page
                browser.Single("#pageA").Click();

                // verify items count
                browser.FindElements("ul#first li").ThrowIfDifferentCountThan(3);

                // verify first page values
                browser.First("input[type=text]").GetAttribute("value").Contains("Hello");
                browser.Last("input[type=text]").GetAttribute("value").Contains("1");
                AssertUI.InnerTextEquals(browser.First("#test2"), "A");
            });
        }

        public SPAViewModelReapplicationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
