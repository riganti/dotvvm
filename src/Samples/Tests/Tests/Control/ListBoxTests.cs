using DotVVM.Samples.Tests.Base;
using Riganti.Selenium.Core;
using DotVVM.Testing.Abstractions;
using Xunit;
using Xunit.Abstractions;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium;
using Riganti.Selenium.Core.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class ListBoxTests : AppSeleniumTest
    {
        public ListBoxTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Control_ListBox()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ListBox_ListBox);

                var result = browser.Single("[data-ui=result]");
                AssertUI.InnerTextEquals(result, "0");

                AssertUI.Attribute(browser.Single("select[data-ui=single]"), "size", "4");

                var firstOption = browser.ElementAt("select[data-ui=single] option", 0);

                AssertUI.InnerTextEquals(firstOption, "Too long text");
                AssertUI.Attribute(firstOption, "title", "Nice title");

                firstOption.Click();
                AssertUI.InnerTextEquals(result, "1");

                var secondOption = browser.ElementAt("select[data-ui=single] option", 1);

                AssertUI.InnerTextEquals(secondOption, "Text1");
                AssertUI.Attribute(secondOption, "title", "Even nicer title");

                secondOption.Click();
                AssertUI.InnerTextEquals(result, "2");
            });
        }

        [Fact]
        public void Control_MultiSelect()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ListBox_ListBox);

                var initialSelectedElements = browser.FindElements("li");
                Assert.Equal(0, initialSelectedElements.Count);

                AssertUI.HasAttribute(browser.Single("select[data-ui=multiple]"), "multiple");

                var firstOption = browser.ElementAt("select[data-ui=multiple] option", 0);
                var thirdOption = browser.ElementAt("select[data-ui=multiple] option", 2);
                var fourOption = browser.ElementAt("select[data-ui=multiple] option", 3);

                AssertUI.InnerTextEquals(firstOption, "Too long text");
                AssertUI.Attribute(firstOption, "title", "Nice title");

                firstOption.Click();
                CtrlClick(browser, thirdOption);
                CtrlClick(browser, fourOption);

                var selectedElements = browser.FindElements("li");
                Assert.Equal(3, selectedElements.Count);

                AssertUI.InnerTextEquals(selectedElements[0], "1");
                AssertUI.InnerTextEquals(selectedElements[1], "3");
                AssertUI.InnerTextEquals(selectedElements[2], "4");
            });
        }

        private static void CtrlClick(IBrowserWrapper browser, IElementWrapper thirdOption) => new Actions(browser.Driver)
                            .KeyDown(Keys.Control)
                            .Click(thirdOption.WebElement)
                            .KeyUp(Keys.Control)
                            .Perform();
    }
}
