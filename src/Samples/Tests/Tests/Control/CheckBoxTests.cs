using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Newtonsoft.Json;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class CheckBoxTests : AppSeleniumTest
    {
        [Fact]
        public void Control_CheckBox_CheckBox()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_CheckBox);

                var boxes = browser.FindElements("fieldset");

                // single check box
                boxes.ElementAt(0).First("input[type=checkbox]").Click();
                boxes.ElementAt(0).First("input[type=button]").Click();

                AssertUI.InnerTextEquals(boxes.ElementAt(0).First("span.result")
                    , "True");

                // check box list
                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 1).Click();
                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 2).Click();
                boxes.ElementAt(1).First("input[type=button]").Click();

                AssertUI.InnerTextEquals(boxes.ElementAt(1).First("span.result")
                    , "g, b");

                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 2).Click();
                boxes.ElementAt(1).ElementAt("input[type=checkbox]", 0).Click();
                boxes.ElementAt(1).First("input[type=button]").Click();

                AssertUI.InnerTextEquals(boxes.ElementAt(1).First("span.result")
                    , "g, r");

                // checked changed
                boxes.ElementAt(2).ElementAt("input[type=checkbox]", 0).Click();

                AssertUI.InnerTextEquals(boxes.ElementAt(2).Last("span.result")
                    , "1");
                AssertUI.IsChecked(boxes.ElementAt(2).First("input[type=checkbox]"));

                boxes.ElementAt(2).ElementAt("input[type=checkbox]", 0).Click();

                AssertUI.InnerTextEquals(boxes.ElementAt(2).Last("span.result")
                    , "2");
                AssertUI.IsNotChecked(boxes.ElementAt(2).First("input[type=checkbox]"));

                // checked visible
                var v = boxes.ElementAt(4);
                AssertUI.IsDisplayed(boxes.ElementAt(4).ElementAt("input[type=checkbox]", 0));
                AssertUI.IsNotDisplayed(boxes.ElementAt(4).ElementAt("input[type=checkbox]", 1));

                boxes.ElementAt(4).Single("input[data-ui=switch]").Click();

                AssertUI.IsNotDisplayed(boxes.ElementAt(4).ElementAt("input[type=checkbox]", 0));
                AssertUI.IsDisplayed(boxes.ElementAt(4).ElementAt("input[type=checkbox]", 1));

                // dataContext change
                boxes.ElementAt(5).First("input[type=checkbox]").Click();
                AssertUI.InnerTextEquals(boxes.ElementAt(5).First("span.result")
                    , "true");
            });
        }

        [Fact]
        public void Control_CheckBox_InRepeater()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_InRepeater);

                var repeater = browser.Single("div[data-ui='repeater']");
                var checkBoxes = browser.FindElements("label[data-ui='checkBox']");

                var checkBox = checkBoxes.ElementAt(0).Single("input");
                checkBox.Click();
                AssertUI.IsChecked(checkBox);
                AssertUI.InnerText(browser.Single("span[data-ui='selectedColors']"), s => s.Contains("orange"));

                checkBox = checkBoxes.ElementAt(1).Single("input");
                checkBox.Click();
                AssertUI.IsChecked(checkBox);
                AssertUI.InnerText(browser.Single("span[data-ui='selectedColors']"), s => s.Contains("orange") && s.Contains("red"));

                checkBox = checkBoxes.ElementAt(2).Single("input");
                checkBox.Click();
                AssertUI.IsChecked(checkBox);
                AssertUI.InnerText(browser.Single("span[data-ui='selectedColors']"), s => s.Contains("orange") && s.Contains("red") && s.Contains("black"));

                checkBoxes = browser.FindElements("label[data-ui='checkBox']");

                browser.First("[data-ui='set-server-values']").Click();
                AssertUI.IsChecked(checkBoxes.ElementAt(0).Single("input"));
                AssertUI.IsChecked(checkBoxes.ElementAt(2).Single("input"));
                AssertUI.InnerText(browser.Single("span[data-ui='selectedColors']"), s => s.Contains("orange") && s.Contains("black"));
            });
        }

        [Fact]
        public void Control_CheckBox_CheckedItemsNull()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_CheckedItemsNull);
            });
        }

        [Fact]
        public void Control_CheckBox_Indeterminate()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_Indeterminate);

                bool isIndeterminate(string id) =>
                    (bool)browser.GetJavaScriptExecutor().ExecuteScript($"return document.getElementById({JsonConvert.ToString(id)}).indeterminate");

                var checkBox = browser.First("#checkbox-indeterminate");
                var reset = browser.First("input[type=button]");
                var value = browser.First("span.value");

                AssertUI.InnerTextEquals(value, "Indeterminate");
                Assert.True(isIndeterminate("checkbox-indeterminate"), "The checkbox should be in indeterminate state.");
                Assert.False(isIndeterminate("checkbox-no-indeterminate"), "The checkbox should be unchecked, not in indetermined state.");

                checkBox.Click();
                AssertUI.InnerTextEquals(value, "Other");
                Assert.False(isIndeterminate("checkbox-indeterminate"), "The checkbox should not be in indeterminate state anymore.");
                AssertUI.IsChecked(checkBox);
                AssertUI.IsChecked(browser.First("#checkbox-no-indeterminate"));

                reset.Click();
                AssertUI.InnerTextEquals(value, "Indeterminate");
                Assert.True(isIndeterminate("checkbox-indeterminate"), "The checkbox should be in indeterminate state.");
                AssertUI.IsNotChecked(browser.First("#checkbox-no-indeterminate"));
            });
        }

        [Fact(Skip = "this test just does not work on any dotvvm version")]
        public void Control_CheckBox_CheckedItemsEmpty()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_CheckedItemsRepeater);
                browser.WaitUntilDotvvmInited();

                var checkBoxes = browser.FindElements("checkbox", SelectByDataUi);
                Assert.Equal(0, checkBoxes.Count);
                void UpdateData() => browser.WaitFor(() => {
                    var button = browser.First("btn-update", SelectByDataUi).Click();
                    var repeater = browser.Single("repeater", SelectByDataUi);
                    checkBoxes = repeater.FindElements("input", SelectBy.TagName);
                    Assert.Equal(2, checkBoxes.Count);
                    AssertUI.IsChecked(checkBoxes[0]);
                    AssertUI.IsNotChecked(checkBoxes[1]);
                }, 10000, "Error!");
                UpdateData();
                checkBoxes[0].Click();
                checkBoxes[1].Click();
                UpdateData();
            });
        }

        [Fact]
        public void Control_CheckBox_CheckBoxObjects()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_CheckBox_Objects);

                var checkboxes = browser.FindElements("input[type=checkbox]");
                var ul = browser.Single("ul");

                AssertUI.IsChecked(checkboxes[0]);
                AssertUI.IsNotChecked(checkboxes[1]);
                AssertUI.IsNotChecked(checkboxes[2]);
                ul.FindElements("li").ThrowIfDifferentCountThan(1);
                AssertUI.TextEquals(ul.ElementAt("li", 0), "1: Red");

                // check second checkbox
                checkboxes[1].Click();
                ul.FindElements("li").ThrowIfDifferentCountThan(2);
                AssertUI.TextEquals(ul.ElementAt("li", 0), "1: Red");
                AssertUI.TextEquals(ul.ElementAt("li", 1), "2: Green");

                // check third check box
                checkboxes[2].Click();
                ul.FindElements("li").ThrowIfDifferentCountThan(3);
                AssertUI.TextEquals(ul.ElementAt("li", 0), "1: Red");
                AssertUI.TextEquals(ul.ElementAt("li", 1), "2: Green");
                AssertUI.TextEquals(ul.ElementAt("li", 2), "3: Blue");

                // uncheck second checkbox
                checkboxes[1].Click();
                ul.FindElements("li").ThrowIfDifferentCountThan(2);
                AssertUI.TextEquals(ul.ElementAt("li", 0), "1: Red");
                AssertUI.TextEquals(ul.ElementAt("li", 1), "3: Blue");

                // click button
                browser.Single("input[type=button]").Click().Wait();
                AssertUI.IsNotChecked(checkboxes[0]);
                AssertUI.IsChecked(checkboxes[1]);
                AssertUI.IsNotChecked(checkboxes[2]);
                ul.FindElements("li").ThrowIfDifferentCountThan(1);
                AssertUI.TextEquals(ul.ElementAt("li", 0), "2: Green");
            });
        }

        public CheckBoxTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
