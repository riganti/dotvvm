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
        public void Control_ComboBox_Default()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_Default);

                var comboBox = browser.First("hardcoded-combobox", SelectByDataUi);
                var selectedValue = browser.First("selected-value", SelectByDataUi);

                AssertUI.IsDisplayed(comboBox.Select(0));
                AssertUI.InnerTextEquals(selectedValue, "1");

                // select second option from combobox
                comboBox.Select(1);
                AssertUI.InnerTextEquals(selectedValue, "2");

                // select third option from combobox
                comboBox.Select(2);
                AssertUI.InnerTextEquals(selectedValue, "3");

                // select fourth option from combobox
                comboBox.Select(3);
                AssertUI.InnerTextEquals(selectedValue, "4");
            });
        }

        [Fact]
        [SampleReference(SamplesRouteUrls.ControlSamples_ComboBox_Default)]
        public void Control_ComboBox_ComboBoxBinded()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_Default);

                var comboBox = browser.First("binded-combobox", SelectByDataUi);
                var selectedText = browser.First("selected-text", SelectByDataUi);

                AssertUI.IsDisplayed(comboBox.Select(0));
                AssertUI.InnerTextEquals(selectedText, "A");

                // select second option from combobox
                comboBox.Select(1);
                AssertUI.InnerTextEquals(selectedText, "AA");

                // select third option from combobox
                comboBox.Select(2);
                AssertUI.InnerTextEquals(selectedText, "AAA");

                // select fourth option from combobox
                comboBox.Select(3);
                AssertUI.InnerTextEquals(selectedText, "AAAA");
            });
        }

        [Fact]
        public void Control_ComboBox_DelaySync()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_DelaySync);

                // check that the second item is selected in both ComboBoxes on the page start
                AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 1));
                AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 1));

                // change the DataSource collection on the server and verify that the second item is selected in both ComboBoxes
                browser.First("input").Click();

                AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 1));
                AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 1));
            });
        }

        [Fact]
        public void Control_ComboBox_DelaySync2()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_DelaySync2);
                browser.First("input[type=button]").Click();

                // check the comboboxes
                AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 0));
                AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 1));

                // check the labels
                AssertUI.InnerTextEquals(browser.ElementAt(".result", 0), "1");
                AssertUI.InnerTextEquals(browser.ElementAt(".result", 1), "2");
            });
        }

        [Fact]
        public void Control_ComboBox_Title()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_Title);

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

                // null value
                var span = browser.Single("selected-value", SelectByDataUi);
                AssertUI.InnerTextEquals(span, "");

                // check combobox works
                var combobox = browser.Single("combobox", SelectByDataUi);
                combobox.Select(0);
                AssertUI.InnerTextEquals(span, "First");

                // test buttons
                browser.ElementAt("input[type=button]", 0).Click();
                AssertUI.InnerTextEquals(span, "First");
                AssertUI.IsSelected(combobox.FindElements("option")[0]);

                browser.ElementAt("input[type=button]", 1).Click();
                AssertUI.InnerTextEquals(span, "");
                AssertUI.IsNotSelected(combobox.FindElements("option")[0]);
                AssertUI.IsNotSelected(combobox.FindElements("option")[1]);
                AssertUI.IsNotSelected(combobox.FindElements("option")[2]);
                
                browser.ElementAt("input[type=button]", 2).Click();
                AssertUI.InnerTextEquals(span, "First");
                AssertUI.IsSelected(combobox.FindElements("option")[0]);

                browser.ElementAt("input[type=button]", 3).Click();
                AssertUI.InnerTextEquals(span, "Second");
                AssertUI.IsSelected(combobox.FindElements("option")[1]);
            });
        }

        [Fact]
        public void Control_ComboBox_ItemBinding_ItemValueBinding_Complex_Error()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ItemBinding_ItemValueBinding_Complex_Error);

                AssertUI.InnerText(browser.First(".exceptionMessage"), s => s.Contains("Return type") && s.Contains("ItemValueBinding") && s.Contains("primitive type"));
                AssertUI.InnerText(browser.First("p.summary"), s => s.Contains("DotVVM.Framework.Compilation.DotvvmCompilationException"));
                AssertUI.InnerText(browser.First(".errorUnderline"), s => s.Contains("ItemValueBinding=") && s.Contains("{value:"));
            });
        }

        [Fact]
        public void Control_ComboBox_ItemBinding_ItemValueBinding_Enum()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ItemBinding_ItemValueBinding_Enum);

                var value = browser.Single("value", SelectByDataUi);
                var dropDown = browser.Single("complex-crash", SelectByDataUi);
                var dropDownButtons = dropDown.FindElements("option", OpenQA.Selenium.By.TagName);

                AssertUI.InnerTextEquals(value,"EValue1");

                for (int i = 0; i < dropDownButtons.Count; i++)
                {
                    dropDown.Click();
                    dropDownButtons.ElementAt(i).Click();

                    AssertUI.InnerTextEquals(value, "EValue" + ((i % 3) + 1).ToString());
                }
            });
        }

        [Fact]
        public void Control_ComboBox_ItemBinding_ItemValueBinding_Number()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ItemBinding_ItemValueBinding_Number);
               
                var value = browser.Single("value", SelectByDataUi);
                var dropDown = browser.Single("complex-crash", SelectByDataUi);
                var dropDownButtons = dropDown.FindElements("option", OpenQA.Selenium.By.TagName);

                AssertUI.TextEmpty(value);

                for (int i = 0; i < dropDownButtons.Count; i++)
                {
                    dropDown.Click();
                    dropDownButtons.ElementAt(i).Click();

                    AssertUI.InnerTextEquals(value, (i + 1).ToString());
                }
            });
        }

        [Fact]
        public void Control_ComboBox_ItemBinding_ItemValueBinding_SelectedValue_Complex_Error()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ItemBinding_ItemValueBinding_SelectedValue_ComplexToInt_Error);

                AssertUI.InnerText(browser.First(".exceptionMessage"), s => s.Contains("DotVVM.Samples.Common.ViewModels.ControlSamples.ComboBox.ComboxItemBindingViewModel+ComplexType") && s.Contains("not assignable") && s.Contains("System.Int32"));
                AssertUI.InnerText(browser.First("p.summary"), s => s.Contains("DotVVM.Framework.Compilation.DotvvmCompilationException"));
                AssertUI.InnerText(browser.First(".errorUnderline"), s => s.Contains("{value: SelectedInt}"));
            });
        }

        [Fact]
        public void Control_ComboBox_ItemBinding_ItemValueBinding_SelectedValue_StringToInt_Error()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ItemBinding_ItemValueBinding_SelectedValue_StringToInt_Error);

                AssertUI.InnerText(browser.First(".exceptionMessage"), s => s.Contains("System.String") && s.Contains("not assignable") && s.Contains("System.Int32"));
                AssertUI.InnerText(browser.First("p.summary"), s => s.Contains("DotVVM.Framework.Compilation.DotvvmCompilationException"));
                AssertUI.InnerText(browser.First(".errorUnderline"), s => s.Contains("{value: SelectedInt}"));
            });
        }

        [Fact]
        public void Control_ComboBox_ItemBinding_ItemValueBinding_String()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ItemBinding_ItemValueBinding_String);

                var value = browser.Single("value", SelectByDataUi);
                var dropDown = browser.Single("complex-crash", SelectByDataUi);
                var dropDownButtons = dropDown.FindElements("option", OpenQA.Selenium.By.TagName);

                AssertUI.TextEmpty(value);

                for (int i = 0; i < dropDownButtons.Count; i++)
                {
                    dropDown.Click();
                    dropDownButtons.ElementAt(i).Click();

                    AssertUI.InnerTextEquals(value, dropDownButtons.ElementAt(i).GetInnerText());
                }
            });
        }

        [Fact]
        public void Control_ComboBox_ItemBinding_ItemValueBinding_StringToObject()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_ItemBinding_ItemValueBinding_StringToObject);

                AssertUI.ContainsElement(browser.Single("body",SelectBy.TagName),"select > option");
            });
        }

        [Fact]
        public void Control_ComboBox_BindingCTValidation_StringToEnum()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ComboBox_BindingCTValidation_StringToEnum);

                var dropDown = browser.Single("string-to-enum", SelectByDataUi);
                var dropDownButtons = dropDown.FindElements("option", SelectBy.TagName);
                var setSecondaryFieldButton = browser.Single("set-secondary-field", SelectByDataUi);
                var enum1 = browser.Single("enum", SelectByDataUi);
                var enum2 = browser.Single("enum2", SelectByDataUi);

                dropDownButtons.ElementAt(1).Click();
                setSecondaryFieldButton.Click();

                for (int i = 0; i < dropDownButtons.Count; i++)
                {
                    dropDownButtons.ElementAt(i).Click();

                    AssertUI.TextEquals(enum1, dropDownButtons.ElementAt(i).GetInnerText());
                    AssertUI.TextNotEquals(enum2, dropDownButtons.ElementAt(i).GetInnerText());

                    setSecondaryFieldButton.Click();
                    AssertUI.TextEquals(enum2, dropDownButtons.ElementAt(i).GetInnerText());
                }
            });
        }

    }
}
