using DotVVM.Samples.Tests.Base;
using Riganti.Selenium.Core;
using DotVVM.Testing.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class ListBoxTests : AppSeleniumTest
    {
        public ListBoxTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Control_ListBox_ListBox()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ListBox_ListBox);

                var result = browser.Single("[data-ui=result]");
                AssertUI.InnerTextEquals(result, "0");

                AssertUI.Attribute(browser.Single("select"), "size", "3");

                var firstOption = browser.ElementAt("select option", 0);

                AssertUI.InnerTextEquals(firstOption, "Too long text");
                AssertUI.Attribute(firstOption, "title", "Nice title");

                firstOption.Click();
                AssertUI.InnerTextEquals(result, "1");

                var secondOption = browser.ElementAt("select option", 1);

                AssertUI.InnerTextEquals(secondOption, "Text1");
                AssertUI.Attribute(secondOption, "title", "Even nicer title");

                secondOption.Click();
                AssertUI.InnerTextEquals(result, "2");
            });
        }
    }
}
