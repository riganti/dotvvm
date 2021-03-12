using System;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DotVVM.Samples.Tests.Control
{
    public class NestedRepeaterTests : AppSeleniumTest
    {
        [Fact]
        public void Control_Repeater_NestedRepeater()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_NestedRepeater);
                

                var result = browser.First("#result");

                browser.ElementAt("a", 0).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(result, "Child 1 Subchild 1");
                }, 5000);

                browser.ElementAt("a", 1).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(result, "Child 1 Subchild 2");
                }, 5000);

                browser.ElementAt("a", 2).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(result, "Child 1 Subchild 3");
                }, 5000);

                browser.ElementAt("a", 3).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(result, "Child 2 Subchild 1");
                }, 5000);

                browser.ElementAt("a", 4).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(result, "Child 2 Subchild 2");
                }, 5000);

                browser.ElementAt("a", 5).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(result, "Child 3 Subchild 1");
                }, 5000);

                browser.ElementAt("a", 6).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(result, "Child 1 Subchild 1");
                }, 5000);

                browser.ElementAt("a", 7).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(result, "Child 1 Subchild 2");
                }, 5000);

                browser.ElementAt("a", 8).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(result, "Child 1 Subchild 3");
                }, 5000);

                browser.ElementAt("a", 9).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(result, "Child 2 Subchild 1");
                }, 5000);

                browser.ElementAt("a", 10).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(result, "Child 2 Subchild 2");
                }, 5000);

                browser.ElementAt("a", 11).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(result, "Child 3 Subchild 1");
                }, 5000);
            });
        }

        [Fact]
        public void Control_Repeater_NestedRepeaterWithControl()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_NestedRepeaterWithControl);
                browser.WaitUntilDotvvmInited();
                browser.Wait(500);

                var result = browser.First("#result");
                var buttons = browser.FindElements("input[type=button]");

                int count = 1;
                foreach (var button in buttons)
                {
                    browser.WaitFor(() => AssertUI.InnerTextEquals(result, count.ToString()), 5000);
                    button?.Click();
                    count++;
                }
            });
        }

        public NestedRepeaterTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
