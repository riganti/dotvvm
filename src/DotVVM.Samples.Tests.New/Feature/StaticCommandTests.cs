
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace DotVVM.Samples.Tests.New.Feature
{
    public class StaticCommandTests : AppSeleniumTest
    {
        public StaticCommandTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_StaticCommand_ValueAssignmentInControl()
        {

            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_ValueAssignmentInControl);

                var intResult = browser.First("#intResult");
                var vmResult = browser.First("#vmResult");
                var stringResult = browser.First("#stringResult");
                var boolResult = browser.First("#boolResult");


                var setTrue = browser.First("#setTrue");
                var setFalse = browser.First("#setFalse");

                var setNull = browser.First("#setNull");
                var setStringValue = browser.First("#setStringValue");

                var setZero = browser.First("#setZero");
                var setTen = browser.First("#setTen");

                var vmSetTrue = browser.First("#vmSetTrue");
                var vmSetFalse = browser.First("#vmSetFalse");


                AssertUI.TextEquals(intResult, "0");
                AssertUI.TextEquals(stringResult, "");
                AssertUI.TextEquals(vmResult, "false");
                AssertUI.TextEquals(boolResult, "false");

                //bool
                setTrue.Click();
                AssertUI.TextEquals(boolResult, "true");
                setFalse.Click();
                AssertUI.TextEquals(boolResult, "false");

                //int
                setTen.Click();
                AssertUI.TextEquals(intResult, "10");
                setZero.Click();
                AssertUI.TextEquals(intResult, "0");

                //string
                setStringValue.Click();
                AssertUI.TextEquals(stringResult, "value");
                setNull.Click();
                AssertUI.TextEquals(stringResult, "");

                //vm value
                vmSetTrue.Click();
                AssertUI.TextEquals(vmResult, "true");
                vmSetFalse.Click();
                AssertUI.TextEquals(vmResult, "false");
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
