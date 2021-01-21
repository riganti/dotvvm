using System;
using System.Collections.Generic;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;

namespace DotVVM.Samples.Tests.Feature
{
    public class ViewModelProtectionTests : AppSeleniumTest
    {
        public const string ChangedText = "The quick brown fox jumps over the lazy dog";
        public const string OriginalText = "Lorem Ipsum Dolor Sit Amet";

        private enum RadioButtonValues
        {
            Red,
            Green,
            Blue
        }

        public ViewModelProtectionTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_ViewModelProtection_ComplexViewModelProtection_SignedString()
        {
            RunComplexViewModelProtectionTest(browser => {
                CheckRadioButtonsState(browser, RadioButtonValues.Red);
                browser.Single("change-color", this.SelectByDataUi).Click();
                CheckRadioButtonsState(browser, RadioButtonValues.Green);
            }, browser => CheckRadioButtonsState(browser, RadioButtonValues.Red));
        }

        [Fact]
        public void Feature_ViewModelProtection_SignedNestedInServerToClient()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ViewModelProtection_SignedNestedInServerToClient);

                AssertUI.InnerTextEquals(browser.First("h1"), "Server Error, HTTP 500: Unhandled exception occurred");
            });
        }

        [Fact]
        public void Feature_ViewModelProtection_NestedSignatures()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ViewModelProtection_NestedSignatures);

                var pre = browser.Single("pre");

                // check that encrypted values are not directly in the viewmodel
                AssertUI.Text(pre, t => !t.Contains("encryptedThing", StringComparison.CurrentCultureIgnoreCase));

                // check that postback works
                browser.ElementAt("input[type=button]", 0).Click();
                browser.WaitFor(() => {
                    AssertUI.Text(pre, t => !t.Contains("encryptedThing", StringComparison.CurrentCultureIgnoreCase));
                }, 2000);

                // change the viewmodel on client side and check that it works
                browser.ElementAt("input[type=button]", 1).Click().Wait();
                AssertUI.Text(pre, t => !t.Contains("\"Next\": {", StringComparison.CurrentCultureIgnoreCase));
                browser.ElementAt("input[type=button]", 0).Click();
                browser.WaitFor(() => {
                    AssertUI.Text(pre, t => t.Contains("\"Next\": {", StringComparison.CurrentCultureIgnoreCase));
                }, 2000);
                AssertUI.Text(pre, t => !t.Contains("encryptedThing", StringComparison.CurrentCultureIgnoreCase));

                // tamper with encrypted values
                browser.GetJavaScriptExecutor().ExecuteScript("dotvvm.viewModels.root.viewModel.$encryptedValues(dotvvm.viewModels.root.viewModel.$encryptedValues()[1] + dotvvm.viewModels.root.viewModel.$encryptedValues()[0] + dotvvm.viewModels.root.viewModel.$encryptedValues().substring(2));");
                browser.ElementAt("input[type=button]", 0).Click();
                browser.WaitFor(() => {
                    AssertUI.IsDisplayed(browser.Single("#debugWindow"));
                }, 2000);
            });
        }

        [Fact]
        public void Feature_ViewModelProtection_ViewModelProtection()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ViewModelProtection_ViewModelProtection);

                // get original value
                var originalValue = browser.First("strong span").GetText();

                // modify protected data
                browser.Last("a").Click();

                // make sure it happened
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(browser.First("strong span"), "hello");
                }, 2000);

                // try to do postback
                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                browser.Click("input[type=button]");
                
                // verify that the original value was restored
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(browser.First("strong span"), originalValue);
                }, 2000);
            });
        }

        [Theory]
        [InlineData("bothMessage", OriginalText, ChangedText, ChangedText)]
        [InlineData("clientToServerMessage", "", ChangedText, ChangedText)]
        [InlineData("ifInPostbackPathMessage", OriginalText, ChangedText, OriginalText)]
        [InlineData("serverToClientFirstRequestMessage", OriginalText, ChangedText, ChangedText)]
        [InlineData("serverToClientPostbackMessage", "", "", OriginalText)]
        public void Feature_ViewModelProtection_ComplexViewModelProtection(string messageDataUi, string originalText, string changedText, string afterPostBackText)
        {
            RunComplexViewModelProtectionTest(browser => {
                var message = browser.Single(messageDataUi, this.SelectByDataUi);
                AssertUI.TextEquals(message, originalText);
                browser.Single($"change-{messageDataUi}", this.SelectByDataUi).Click().Wait();

                message = browser.Single(messageDataUi, this.SelectByDataUi);
                AssertUI.TextEquals(message, changedText);
            },
            browser => {
                var message = browser.Single(messageDataUi, this.SelectByDataUi);
                AssertUI.TextEquals(message, afterPostBackText);
            });
        }

        private void CheckRadioButtonsState(IBrowserWrapper browser, RadioButtonValues selectedColor)
        {
            var radios = new List<IElementWrapper>();
            radios.Add(browser.Single("radio-red", this.SelectByDataUi));
            radios.Add(browser.Single("radio-green", this.SelectByDataUi));
            radios.Add(browser.Single("radio-blue", this.SelectByDataUi));
            var selectedColorElement = browser.Single("selected-color", this.SelectByDataUi);

            int checkedRadioIndex = (int)selectedColor;
            AssertUI.IsChecked(radios[checkedRadioIndex]);
            radios.RemoveAt(checkedRadioIndex);
            radios.ForEach(AssertUI.IsNotChecked);

            AssertUI.TextEquals(selectedColorElement, selectedColor.ToString().ToLower());
        }

        private void RunComplexViewModelProtectionTest(Action<IBrowserWrapper> beforePostback, Action<IBrowserWrapper> afterPostback)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ViewModelProtection_ComplexViewModelProtection);

                beforePostback(browser);
                browser.Single("postback", this.SelectByDataUi).Click();
                browser.Wait(500);
                afterPostback(browser);
            });
        }
    }
}
