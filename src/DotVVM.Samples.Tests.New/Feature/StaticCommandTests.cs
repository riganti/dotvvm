using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.New.Feature
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

        [Fact]
        public void Feature_StaticCommand_StaticCommand()
        {
            RunInAllBrowsers(browser => {
                foreach (var b in browser.FindElements("input[type=button]"))
                {
                    browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand);
                    browser.Wait();
                    b.Click();
                    AssertUI.InnerTextEquals(browser.First("span"), "Hello Deep Thought!");
                }
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_NullBinding))]
        public void Feature_StaticCommand_NullBinding()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_NullBinding);
                browser.Wait();

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
                browser.Wait();

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
            browser.Wait();

            // select second value in the first combo box, the second one should select the second value too 
            browser.ElementAt("select", 0).Select(1).Wait();
            browser.ElementAt("select", 1).Select(1).Wait();
            AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 1));
            AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 1));

            // select third value in the first combo box, the second one should select the third value too 
            browser.ElementAt("select", 0).Select(2).Wait();
            browser.ElementAt("select", 1).Select(2).Wait();
            AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 2));
            AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 2));

            // select first value in the first combo box, the second one should select the first value too 
            browser.ElementAt("select", 0).Select(0).Wait();
            browser.ElementAt("select", 1).Select(0).Wait();
            AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 0));
            AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 0));

            // click the first button - the second value should be selected in the first select, the second select should not change
            browser.ElementAt("input", 0).Click().Wait();
            AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 1));
            AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 0));

            // click the second button - the third value should be selected in the second select, the first select should not change
            browser.ElementAt("input", 1).Click().Wait();
            AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 1));
            AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 2));

            // click the third button - the first value should be selected in the second select, the first select should not change
            browser.ElementAt("input", 2).Click().Wait();
            AssertUI.IsSelected(browser.ElementAt("select", 0).ElementAt("option", 1));
            AssertUI.IsSelected(browser.ElementAt("select", 1).ElementAt("option", 0));
        }
    }
}
