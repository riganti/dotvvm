using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;
using static DotVVM.Samples.Tests.UITestUtils;

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
                AssertPageValues("Hello", "1", "A");

                //check url
                AssertUI.Url(browser, s => s.Contains("SPAViewModelReapplication/page"));

                // try the postback
                browser.First("input[type=button]").Click();
                AssertUI.InnerTextEquals(browser.First("#testResult"), "Hello1");

                // go to the second page
                browser.Single("#pageB").Click();
                AssertPageValues("World", "2", "B");

                // try the postback
                browser.First("input[type=button]").Click();
                AssertUI.InnerTextEquals(browser.First("#testResult"), "World2");

                // go to first page
                browser.Single("#pageA").Click();
                AssertPageValues("Hello", "1", "A");

                void AssertPageValues(string expectedText, string expectedNumber, string expectedPage)
                {
                    WaitForIgnoringStaleElements(() => {
                        browser.FindElements("ul#first li").ThrowIfDifferentCountThan(3, WaitForOptions.Disabled);
                        AssertUI.InnerText(browser.Single("#first"), s => s.Contains("Entry 1") && s.Contains("Entry 2") && s.Contains("Entry 3"));
                        Assert.Contains(expectedText, browser.First("input[type=text]").GetAttribute("value"));
                        Assert.Contains(expectedNumber, browser.Last("input[type=text]").GetAttribute("value"));
                        AssertUI.InnerTextEquals(browser.First("#test2"), expectedPage);
                    });
                }
            });
        }

        public SPAViewModelReapplicationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
