using Riganti.Selenium.Core;
using System;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class IncludeInPagePropertyTests : AppSeleniumTest
    {
        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_IncludeInPageProperty_IncludeInPage))]
        public void Control_IncludeInPageProperty_IncludeInPage_GridView()
        {
            CheckIncludeInPage(browser => {
                var gridView = browser.Single("gridView", this.SelectByDataUi);
                AssertUI.IsDisplayed(gridView);
                AssertUI.ContainsElement(gridView, "thead");
                AssertUI.ContainsElement(gridView, "tbody");
            }, browser => {
                Assert.Equal(0, browser.FindElements("gridView", this.SelectByDataUi).Count);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_IncludeInPageProperty_IncludeInPage))]
        public void Control_IncludeInPageProperty_IncludeInPage_GridViewEmptyDataTemplate()
        {
            const string gridViewDataUi = "gridView-emptyDataTemplate";
            const string messageDataUi = "emptyDataTemplate";

            CheckIncludeInPage(browser => {
                AssertUI.IsNotDisplayed(browser, gridViewDataUi, this.SelectByDataUi);
                var message = browser.Single(messageDataUi, this.SelectByDataUi);
                AssertUI.IsDisplayed(message);
                AssertUI.TextEquals(message, "There are no Customers to display");
            }, browser => {
                Assert.Equal(0, browser.FindElements(gridViewDataUi).Count);
                Assert.Equal(0, browser.FindElements(messageDataUi).Count);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_IncludeInPageProperty_IncludeInPage))]
        public void Control_IncludeInPageProperty_IncludeInPage_Literal()
        {
            CheckIncludeInPage(browser => {
                var literal = browser.Single("literal", this.SelectByDataUi);
                AssertUI.IsDisplayed(literal);
                AssertUI.TextEquals(literal, "Test 1");
            }, browser => {
                Assert.Equal(0, browser.FindElements("literal", this.SelectByDataUi).Count);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_IncludeInPageProperty_IncludeInPage))]
        public void Control_IncludeInPageProperty_IncludeInPage_LiteralsInRepeater()
        {
            CheckIncludeInPage(browser => {
                var literals = browser.FindElements("literal-repeater", this.SelectByDataUi);
                Assert.Equal(3, literals.Count);
                foreach (var literal in literals)
                {
                    AssertUI.IsDisplayed(literal);
                }
            }, browser => {
                Assert.Equal(0, browser.FindElements("literal-repeater", this.SelectByDataUi).Count);
            });
        }

        [Fact]
        public void Control_IncludeInPageProperty_IncludeInPage_RepeaterFirst() => CheckRepeater("repeater-first", 2);

        [Fact]
        public void Control_IncludeInPageProperty_IncludeInPage_RepeaterSecond() => CheckRepeater("repeater-second", 3);

        [Fact]
        public void Control_IncludeInPageProperty_IncludeInPage_TextBox() => CheckTextBox("textbox", "Default text");

        [Fact]
        public void Control_IncludeInPageProperty_IncludeInPage_TextBoxWithDataContext() => CheckTextBox("textbox-dataContext", "John Smith");

        [Fact]
        public void Control_IncludeInPageProperty_IncludeInPage_TextBoxWithVisible() => CheckTextBox("textbox-visible", "Default text", true);

        [Fact]
        public void Control_IncludeInPageProperty_IncludeInPage_TextBoxWithVisibleAndDataContext() => CheckTextBox("textbox-visible-dataContext", "John Smith", true);

        private void CheckIncludeInPage(Action<IBrowserWrapper> beforeSwitch, Action<IBrowserWrapper> afterSwitch)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_IncludeInPageProperty_IncludeInPage);
                beforeSwitch(browser);
                browser.Single("switch-includeInPage", this.SelectByDataUi).Click();
                browser.WaitFor(() => {
                    afterSwitch(browser);
                }, 2000);
            });
        }

        private void CheckRepeater(string dataUi, int childrenCount)
        {
            CheckIncludeInPage(browser => {
                var repeater = browser.First(dataUi, this.SelectByDataUi);
                AssertUI.IsDisplayed(repeater);
                repeater.Children.ThrowIfDifferentCountThan(childrenCount);
            }, browser => {
                browser.FindElements(dataUi, this.SelectByDataUi).ThrowIfDifferentCountThan(0);
            });
        }

        private void CheckTextBox(string dataUi, string text, bool checkVisible = false)
        {
            CheckIncludeInPage(browser => {
                var textBox = browser.Single(dataUi, this.SelectByDataUi);
                AssertUI.TextEquals(textBox, text);
                AssertUI.IsDisplayed(textBox);
                if (checkVisible)
                {
                    var switchVisible = browser.Single("switch-visible", this.SelectByDataUi);
                    switchVisible.Click();
                    AssertUI.IsNotDisplayed(textBox);
                    switchVisible.Click();
                    AssertUI.IsDisplayed(textBox);
                }
            }, browser => {
                browser.FindElements(dataUi, this.SelectByDataUi).ThrowIfDifferentCountThan(0);
            });
        }

        public IncludeInPagePropertyTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
