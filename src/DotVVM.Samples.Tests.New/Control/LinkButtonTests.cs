using DotVVM.Samples.Tests.New;
using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class LinkButtonTests : AppSeleniumTest
    {
        [Fact]
        public void Control_LinkButton_LinkButton()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_LinkButton_LinkButton);

                AssertUI.TagName(browser.First("#ButtonTextProperty"), "a");
                AssertUI.TagName(browser.First("#ButtonTextBinding"), "a");
                AssertUI.TagName(browser.First("#ButtonInnerText"), "a");

                // try to click on a disabled button
                browser.Click("#EnabledLinkButton");
                browser.Wait();
                AssertUI.InnerTextEquals(browser.Last("span"), "0");

                // enable it
                browser.Click("input[type=checkbox]");
                browser.Wait();
                browser.Click("#EnabledLinkButton");
                browser.Wait();
                AssertUI.InnerTextEquals(browser.Last("span"), "1");

                // try to click on a disabled button again
                browser.Click("#EnabledLinkButton");
                browser.Wait();
                AssertUI.InnerTextEquals(browser.Last("span"), "1");
            });
        }

        [Fact]
        public void Control_LinkButton_LinkButtonOnclick()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_LinkButton_LinkButtonOnclick);
                var onclickResult = browser.First("span.result1");
                var clickResult = browser.First("span.result2");
                AssertUI.InnerText(clickResult, s => s.Equals(""));
                AssertUI.InnerText(onclickResult, s => s.Equals(""));

                browser.Click("#LinkButton");
                AssertUI.InnerText(clickResult, s => s.Equals("Changed from command binding"));
                AssertUI.InnerText(onclickResult, s => s.Contains("Changed from onclick"));
            });
        }

        public LinkButtonTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
