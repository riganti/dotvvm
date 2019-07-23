using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class ComboBoxTests : AppSeleniumTest
    {
        public ComboBoxTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Control_ComboBox_ComboBox()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ComboBox);

                var comboBox = browser.First("hardcoded-combobox", SelectByDataUi);
                var selectedValue = browser.First("selected-value", SelectByDataUi);

                AssertUI.IsDisplayed(comboBox.Select(0));
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedValue, "1"), 2000, 30);

                // select second option from combobox
                comboBox.Select(1);
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedValue, "2"), 1000, 30);

                // select third option from combobox
                comboBox.Select(2);
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedValue, "3"), 1000, 30);

                // select fourth option from combobox
                comboBox.Select(3);
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedValue, "4"), 1000, 30);
            });
        }

        [Fact]
        [SampleReference(SamplesRouteUrls.ControlSamples_ComboBox_ComboBox)]
        public void Control_ComboBox_ComboBoxBinded()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ComboBox);

                var comboBox = browser.First("binded-combobox", SelectByDataUi);
                var selectedText = browser.First("selected-text", SelectByDataUi);

                AssertUI.IsDisplayed(comboBox.Select(0));
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedText, "A"), 2000, 30);

                // select second option from combobox
                comboBox.Select(1);
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedText, "AA"), 1000, 30);

                // select third option from combobox
                comboBox.Select(2);
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedText, "AAA"), 1000, 30);

                // select fourth option from combobox
                comboBox.Select(3);
                browser.WaitFor(() => AssertUI.InnerTextEquals(selectedText, "AAAA"), 1000, 30);
            });
        }

        [Fact]
        public void Control_ComboBox_ComboBoxDelaySync()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ComboBoxDelaySync);

                // check that the second item is selected in both ComboBoxes on the page start
                AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 1));
                AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 1));

                // change the DataSource collection on the server and verify that the second item is selected in both ComboBoxes
                browser.First("input").Click();

                browser.WaitFor(() => {
                    AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 1));
                    AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 1));
                }, 800, 30);
            });
        }

        [Fact]
        public void Control_ComboBox_ComboBoxDelaySync2()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ComboBoxDelaySync2);
                browser.First("input[type=button]").Click();

                browser.WaitFor(() => {


                    // check the comboboxes
                    AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 0));
                    AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 1));

                    // check the labels
                    AssertUI.InnerTextEquals(browser.ElementAt(".result", 0), "1");
                    AssertUI.InnerTextEquals(browser.ElementAt(".result", 1), "2");
                }, 1000, 30);
            });
        }

        [Fact]
        public void Control_ComboBox_ComboBoxDelaySync3()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ComboBoxDelaySync3);
                browser.First("input[type=button]").Click();

                browser.WaitFor(() => {
                // check that the combobox appears
                    AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 0));
                }, 1000, 30);
            });
        }

        [Fact]
        public void Control_ComboBox_ComboBoxTitle()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ComboBoxTitle);

                AssertUI.InnerTextEquals(browser.ElementAt("select option", 0), "Too lo...");
                AssertUI.InnerTextEquals(browser.ElementAt("select option", 1), "Text");

                AssertUI.Attribute(browser.ElementAt("select option", 0), "title", "Nice title");
                AssertUI.Attribute(browser.ElementAt("select option", 1), "title", "Even nicer title");
            });
        }

        [Fact]
        public void Control_ComboBox_Nullable()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_Nullable);
                browser.WaitUntilDotvvmInited();

                var span = browser.Single("selected-value", SelectByDataUi);
                // null value
                AssertUI.InnerTextEquals(span, "");

                // check combobox works
                browser.Single("combobox", SelectByDataUi).Select(0);
                browser.WaitFor(() => AssertUI.InnerTextEquals(span, "First"), 1000);
            });
        }
    }
}
