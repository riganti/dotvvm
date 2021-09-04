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
    public class ContentPlaceHolderTests : AppSeleniumTest
    {
        [Fact]
        public void Control_ContentPlaceHolder_ContentPlaceHolderPage()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ContentPlaceHolder_ContentPlaceHolderPage);
                AssertUI.JsPropertyInnerHtml(browser.First("#innerHtmlTest"), html => string.IsNullOrWhiteSpace(System.Net.WebUtility.HtmlDecode(html)), "Inner html has to be empty.");
            });
        }

        [Fact]
        public void Control_ContentPlaceHolder_ContentPlaceHolderPage_ContentTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ContentPlaceHolder_ContentPlaceHolderPage_ContentTest);
                AssertUI.JsPropertyInnerHtml(browser.First("#innerHtmlTest"), html => !string.IsNullOrWhiteSpace(System.Net.WebUtility.HtmlDecode(html)), "Inner html has to contain specified content.");
            });
        }
        [Fact]
        public void Control_ContentPlaceHolder_DoubleContentPlaceHolderPage_ContentTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ContentPlaceHolder_DoubleContentPlaceHolderPage_ContentTest);
                AssertUI.InnerTextEquals(browser.First("title", SelectByDataUi), "Title", failureMessage: "Inner html has to contain specified content.");

                AssertUI.InnerTextEquals(browser.First("content", SelectByDataUi), "Content", failureMessage: "Inner html has to contain specified content.");
            });
        }
        public ContentPlaceHolderTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
