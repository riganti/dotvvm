using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class MultiSelectTests : AppSeleniumTest
    {
        public MultiSelectTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Control_MultiSelect_Binded()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_MultiSelect_binded);

                var multiselect = browser.First("binded-multiselect", SelectByDataUi);
                var selectedValues = browser.First("selected-values", SelectByDataUi);

                AssertUI.IsDisplayed(multiselect.Select(0));
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedValues, "Praha"), 2000, 30);

                // select second option from combobox
                multiselect.Select(1);
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedValues, "Praha Brno"), 1000, 30);

                // select third option from combobox
                multiselect.Select(2);
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedValues, "Praha Brno Napajedla"), 1000, 30);

                // select third option from combobox
                multiselect.Children[0].Click();
                multiselect.Children[1].Click();
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedValues, "Napajedla"), 1000, 30);
            });
        }

        [Fact]
        public void Control_MultiSelect_Hardcoded()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_MultiSelect_hardcoded);

                var multiselect = browser.First("hardcoded-multiselect", SelectByDataUi);
                var selectedValues = browser.First("selected-values", SelectByDataUi);

                AssertUI.IsDisplayed(multiselect.Select(0));
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedValues, "1"), 2000, 30);

                // select second option from combobox
                multiselect.Select(1);
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedValues, "1 2"), 1000, 30);

                // select third option from combobox
                multiselect.Select(2);
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedValues, "1 2 3"), 1000, 30);

                // select third option from combobox
                multiselect.Children[0].Click();
                multiselect.Children[1].Click();
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedValues, "3"), 1000, 30);
            });
        }
    }
}
