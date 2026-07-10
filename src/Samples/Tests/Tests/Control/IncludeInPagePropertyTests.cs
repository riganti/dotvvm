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
        [Theory, InlineData(true), InlineData(false)]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_IncludeInPageProperty_IncludeInPage))]
        public void Control_IncludeInPageProperty_IncludeInPage_GridView(bool defaultValue)
        {
            CheckIncludeInPage(defaultValue, browser => {
                var gridView = browser.Single("gridView", this.SelectByDataUi);
                AssertUI.IsDisplayed(gridView);
                AssertUI.ContainsElement(gridView, "thead");
                AssertUI.ContainsElement(gridView, "tbody");
            }, browser => {
                Assert.Empty(browser.FindElements("gridView", this.SelectByDataUi));
            });
        }

        [Theory, InlineData(true), InlineData(false)]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_IncludeInPageProperty_IncludeInPage))]
        public void Control_IncludeInPageProperty_IncludeInPage_GridViewEmptyDataTemplate(bool defaultValue)
        {
            const string gridViewDataUi = "gridView-emptyDataTemplate";
            const string messageDataUi = "emptyDataTemplate";

            CheckIncludeInPage(defaultValue, browser => {
                AssertUI.IsNotDisplayed(browser, gridViewDataUi, this.SelectByDataUi);
                var message = browser.Single(messageDataUi, this.SelectByDataUi);
                AssertUI.IsDisplayed(message);
                AssertUI.TextEquals(message, "There are no Customers to display");
            }, browser => {
                Assert.Empty(browser.FindElements(gridViewDataUi));
                Assert.Empty(browser.FindElements(messageDataUi));
            });
        }

        [Theory, InlineData(true), InlineData(false)]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_IncludeInPageProperty_IncludeInPage))]
        public void Control_IncludeInPageProperty_IncludeInPage_Literal(bool defaultValue)
        {
            CheckIncludeInPage(defaultValue, browser => {
                var literal = browser.Single("literal", this.SelectByDataUi);
                AssertUI.IsDisplayed(literal);
                AssertUI.TextEquals(literal, "Test 1");
            }, browser => {
                Assert.Empty(browser.FindElements("literal", this.SelectByDataUi));
            });
        }

        [Theory, InlineData(true), InlineData(false)]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_IncludeInPageProperty_IncludeInPage))]
        public void Control_IncludeInPageProperty_IncludeInPage_LiteralsInRepeater(bool defaultValue)
        {
            CheckIncludeInPage(defaultValue, browser => {
                var literals = browser.FindElements("literal-repeater", this.SelectByDataUi);
                Assert.Equal(3, literals.Count);
                foreach (var literal in literals)
                {
                    AssertUI.IsDisplayed(literal);
                }
            }, browser => {
                Assert.Empty(browser.FindElements("literal-repeater", this.SelectByDataUi));
            });
        }

        [Theory]
        [InlineData(true, "repeater-first", 2)]
        [InlineData(false, "repeater-first", 2)]
        [InlineData(true, "repeater-second", 3)]
        [InlineData(false, "repeater-second", 3)]
        public void Control_IncludeInPage_Repeater(bool defaultValue, string dataUi, int childrenCount)
        {
            CheckIncludeInPage(defaultValue, browser => {
                var repeater = browser.First(dataUi, this.SelectByDataUi);
                AssertUI.IsDisplayed(repeater);
                repeater.Children.ThrowIfDifferentCountThan(childrenCount);
            }, browser => {
                browser.FindElements(dataUi, this.SelectByDataUi).ThrowIfDifferentCountThan(0);
            });
        }

        [Theory]
        [InlineData(true, "textbox", "Default text")]
        [InlineData(false, "textbox", "Default text")]
        [InlineData(true, "textbox-dataContext", "John Smith", false)]
        [InlineData(false, "textbox-dataContext", "John Smith", false)]
        [InlineData(true, "textbox-visible", "Default text", true)]
        [InlineData(false, "textbox-visible", "Default text", true)]
        [InlineData(true, "textbox-visible-dataContext", "John Smith", true)]
        [InlineData(false, "textbox-visible-dataContext", "John Smith", true)]
        public void Control_IncludeInPage_TextBox(bool defaultValue, string dataUi, string text, bool checkVisible = false)
        {
            CheckIncludeInPage(defaultValue, browser => {
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

        private void CheckIncludeInPage(bool defaultValue, Action<IBrowserWrapper> whenIncluded, Action<IBrowserWrapper> whenExcluded)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_IncludeInPageProperty_IncludeInPage + $"?default={defaultValue}");

                var (first, second) = defaultValue ? (whenIncluded, whenExcluded) : (whenExcluded, whenIncluded);
                first(browser);

                browser.Single("switch-includeInPage", this.SelectByDataUi).Click();
                browser.WaitFor(() => { second(browser); }, 2000);

                browser.Single("switch-includeInPage", this.SelectByDataUi).Click();
                browser.WaitFor(() => { first(browser); }, 2000);
            });
        }

        public IncludeInPagePropertyTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
