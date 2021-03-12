using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class StaticCommandTests : AppSeleniumTest
    {
        public StaticCommandTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_ValueAssignmentInControl))]
        public void Feature_StaticCommand_ValueAssignmentInControl()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_ValueAssignmentInControl);

                var result = browser.First("#result");

                var setText = browser.First("#setText");
                var setNull = browser.First("#setNull");

                //default
                AssertUI.TextEquals(result, "");

                setText.Click();
                AssertUI.TextEquals(result, "text");

                setNull.Click();
                AssertUI.TextEquals(result, "");

                setText.Click();
                AssertUI.TextEquals(result, "text");
            });
        }

        [Theory]
        [InlineData("compute-static-method")]
        [InlineData("compute-string")]
        [InlineData("compute-service-async")]
        [InlineData("compute-service")]
        public void Feature_StaticCommand_StaticCommand(string selector)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand);
                browser.IsDotvvmPage();

                var b = browser.First(selector, SelectByDataUi);
                b.Click();
                AssertUI.InnerTextEquals(browser.First("span"), "Hello Deep Thought!");
            });
        }

        [Theory]
        [InlineData("service-null")]
        [InlineData("static-null")]
        [InlineData("null")]
        public void Feature_StaticCommand_StaticCommand_ObjectAssignation(string selector)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand);
                browser.IsDotvvmPage();

                var name = browser.First("name", SelectByDataUi);
                var value = browser.First("value", SelectByDataUi);

                var a = browser.First("service-object", SelectByDataUi);
                a.Click();

                AssertUI.TextEquals(name, "Hello DotVVM!");
                AssertUI.TextEquals(value, "1");

                var b = browser.First(selector, SelectByDataUi);
                b.Click();

                AssertUI.TextEmpty(name);
                AssertUI.TextEmpty(value);

                a.Click();
                AssertUI.TextEquals(name, "Hello DotVVM!");
                AssertUI.TextEquals(value, "1");

                b.Click();
                AssertUI.TextEmpty(name);
                AssertUI.TextEmpty(value);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_NullBinding))]
        public void Feature_StaticCommand_NullBinding()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_NullBinding);
                browser.IsDotvvmPage();

                var showSelected = browser.First("#show-selected");
                AssertUI.IsNotDisplayed(showSelected);

                browser.First("#listObject > input:nth-child(2)").Click();
                AssertUI.IsDisplayed(showSelected);
                AssertUI.InnerTextEquals(showSelected, "Hello 2");

                browser.First("#listObject > input:nth-child(3)").Click();
                AssertUI.IsDisplayed(showSelected);
                AssertUI.IsDisplayed(showSelected);
                AssertUI.InnerTextEquals(showSelected, "Hello 3");

                browser.First("#listObject > input:nth-child(4)").Click();
                AssertUI.IsNotDisplayed(showSelected);

                browser.First("#listObject > input:nth-child(1)").Click();

                AssertUI.IsDisplayed(showSelected);
                AssertUI.InnerTextEquals(showSelected, "Hello 1");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_NullAssignment))]
        public void Feature_StaticCommand_NullAssignment()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_NullAssignment);
                browser.IsDotvvmPage();

                void TestSection(string sectionName)
                {
                    AssertUI.InnerTextEquals(browser.Single($".type-{sectionName} .item1"), "not null");
                    AssertUI.InnerTextEquals(browser.Single($".type-{sectionName} .item2"), "not null");
                    browser.Single($".type-{sectionName} input[type=button]").Click();
                    AssertUI.InnerTextEquals(browser.Single($".type-{sectionName} .item1"), "null");
                    AssertUI.InnerTextEquals(browser.Single($".type-{sectionName} .item2"), "null");
                }

                TestSection("int");
                TestSection("string");
                TestSection("datetime");
                TestSection("object");
            });
        }


        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_ComboBoxSelectionChanged))]
        public void Feature_StaticCommand_ComboBoxSelectionChanged()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_ComboBoxSelectionChanged);
                Feature_StaticCommand_ComboBoxSelectionChangedViewModel_Core(browser);
            });
        }

        [Fact]
        public void Feature_StaticCommand_StaticCommand_ComboBoxSelectionChanged_Objects()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_ComboBoxSelectionChanged_Objects);
                Feature_StaticCommand_ComboBoxSelectionChangedViewModel_Core(browser);
            });
        }

        private static void Feature_StaticCommand_ComboBoxSelectionChangedViewModel_Core(IBrowserWrapper browser)
        {
            browser.IsDotvvmPage();

            // select second value in the first combo box, the second one should select the second value too 
            browser.ElementAt("select", 0).Select(1);
            browser.ElementAt("select", 1).Select(1);
            AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 1));
            AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 1));

            // select third value in the first combo box, the second one should select the third value too 
            browser.ElementAt("select", 0).Select(2);
            browser.ElementAt("select", 1).Select(2);
            AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 2));
            AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 2));

            // select first value in the first combo box, the second one should select the first value too 
            browser.ElementAt("select", 0).Select(0);
            browser.ElementAt("select", 1).Select(0);
            AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 0));
            AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 0));

            // click the first button - the second value should be selected in the first select, the second select should not change
            browser.ElementAt("input", 0).Click();
            AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 1));
            AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 0));

            // click the second button - the third value should be selected in the second select, the first select should not change
            browser.ElementAt("input", 1).Click();
            AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 1));
            AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 2));

            // click the third button - the first value should be selected in the second select, the first select should not change
            browser.ElementAt("input", 2).Click();
            AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 1));
            AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 0));
        }

        [Fact]
        public void Feature_StaticCommand_StaticCommand_TaskSequence()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_TaskSequence);

                var textBox = browser.Single("input[type=text]");
                var commandButton = browser.ElementAt("input[type=button]", 0);
                var staticCommandButton = browser.ElementAt("input[type=button]", 1);
                var internalSequenceStaticCommandButton = browser.ElementAt("input[type=button]", 2);
                void resetTextbox()
                {
                    textBox.Clear();
                    textBox.SendKeys("0");
                    textBox.SendKeys(Keys.Tab);
                }

                commandButton.Click();
                browser.WaitFor(() => {
                    AssertUI.Value(textBox, "55");
                }, 500);


                resetTextbox();
                staticCommandButton.Click();

                browser.WaitFor(() => {
                    AssertUI.Value(textBox, "55");
                }, 500);

                resetTextbox();
                internalSequenceStaticCommandButton.Click();

                browser.WaitFor(() => {
                    AssertUI.Value(textBox, "2");
                }, 500);
            });
        }

        [Fact]
        public void Feature_StaticCommand_StaticCommand_ArrayAssigment()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_ArrayAssigment);

                AssertUI.InnerTextEquals(browser.ElementAt(".item", 0), "Anne");
                AssertUI.InnerTextEquals(browser.ElementAt(".item", 1), "Martin");

                var button = browser.Single("input[type=button]");
                button.Click();

                AssertUI.InnerTextEquals(browser.ElementAt(".item", 0), "Bob");
                AssertUI.InnerTextEquals(browser.ElementAt(".item", 1), "Oliver");
                AssertUI.InnerTextEquals(browser.ElementAt(".item", 2), "Pablo");
            });
        }

        [Fact]
        public void Feature_StaticCommand_StaticCommand_LoadComplexDataFromService()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_LoadComplexDataFromService);

                var textBox = browser.Single("input[type=text]");
                textBox.SendKeys("Vindaloo");

                var button = browser.Single("input[type=button]");
                button.Click();

                AssertUI.InnerTextEquals(browser.ElementAt(".name", 0), "Martin");
                AssertUI.InnerTextEquals(browser.ElementAt(".name", 1), "Roman");
                AssertUI.InnerTextEquals(browser.ElementAt(".name", 2), "Igor");

                AssertUI.InnerTextEquals(browser.ElementAt(".food", 0), "Burger");
                AssertUI.InnerTextEquals(browser.ElementAt(".food", 1), "Pizza");
                AssertUI.InnerTextEquals(browser.ElementAt(".food", 2), "Vindaloo");
            });
        }

        [Fact]
        public void Feature_StaticCommand_CustomAwaiter()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_CustomAwaitable);

                var testButton = browser.First("[data-ui=test]");
                var clearButton = browser.First("[data-ui=clear]");

                clearButton.Click();
                testButton.Click();

                browser.WaitFor(() => {
                    var testElement = browser.First("[data-ui=result]");
                    Assert.Equal("Test ok", testElement.GetInnerText());
                }, 1000);
            });
        }
    }
}
