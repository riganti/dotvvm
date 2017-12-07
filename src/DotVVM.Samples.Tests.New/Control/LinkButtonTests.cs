using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Selenium.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using DotVVM.Samples.Tests.New;
using DotVVM.Testing.Abstractions;
using Xunit.Abstractions;
using Xunit;

namespace DotVVM.Samples.Tests.Control
{
    public class LinkButtonTests : AppSeleniumTest
    {

        public LinkButtonTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Control_LinkButton_LinkButton()
        {
            RunInAllBrowsers(browser =>
            {
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
        public void Control_LinkButton_LinkButtonOnClick()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_LinkButton_LinkButtonOnclick);
                var onclickResult = browser.First("span.result1");
                var clickResult = browser.First("span.result2");
                AssertUI.InnerTextEquals(clickResult, "");
                AssertUI.InnerTextEquals(onclickResult, "");

                browser.Click("#LinkButton");
                AssertUI.InnerTextEquals(clickResult, "Changed from command binding");
                AssertUI.InnerTextEquals(onclickResult, "Changed from onclick");
            });
        }
    }
}