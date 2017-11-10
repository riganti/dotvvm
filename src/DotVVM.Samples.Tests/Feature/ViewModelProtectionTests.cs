
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Collections.Generic;
using DotVVM.Testing.Abstractions;
using Riganti.Utils.Testing.Selenium.Core.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
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

        [TestMethod]
        public void Feature_ViewModelProtection_ComplexViewModelProtection_BothMessage() => 
            CheckMessage("bothMessage", OriginalText, ChangedText, ChangedText);

        [TestMethod]
        public void Feature_ViewModelProtection_ComplexViewModelProtection_ClientToServerMessage() => 
            CheckMessage("clientToServerMessage", "", ChangedText, ChangedText);

        [TestMethod]
        public void Feature_ViewModelProtection_ComplexViewModelProtection_IfInPostbackPathMessage() => 
            CheckMessage("ifInPostbackPathMessage", OriginalText, ChangedText, OriginalText);

        [TestMethod]
        public void Feature_ViewModelProtection_ComplexViewModelProtection_ServerToClientFirstRequestMessage() => 
            CheckMessage("serverToClientFirstRequestMessage", OriginalText, ChangedText, ChangedText);

        [TestMethod]
        public void Feature_ViewModelProtection_ComplexViewModelProtection_ServerToClientMessage() => 
            CheckMessage("serverToClientMessage", OriginalText, ChangedText, OriginalText);

        [TestMethod]
        public void Feature_ViewModelProtection_ComplexViewModelProtection_ServerToClientPostbackMessage() => 
            CheckMessage("serverToClientPostbackMessage", "", "", OriginalText);

        [TestMethod]
        public void Feature_ViewModelProtection_ComplexViewModelProtection_SignedString()
        {
            RunComplexViewModelProtectionTest(browser =>
            {
                CheckRadioButtonsState(browser, RadioButtonValues.Red);
                browser.Single("change-color", this.SelectByDataUi).Click();
                CheckRadioButtonsState(browser, RadioButtonValues.Green);
            }, browser => CheckRadioButtonsState(browser, RadioButtonValues.Red));
        }

        [TestMethod]
        public void Feature_ViewModelProtection_SignedNestedInServerToClient()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ViewModelProtection_SignedNestedInServerToClient);

                browser.First("h1").CheckIfTextEquals("Server Error, HTTP 500: Unhandled exception occured");
            });
        }
        [TestMethod]
        public void Feature_ViewModelProtection_ViewModelProtection()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ViewModelProtection_ViewModelProtection);

                // get original value
                var originalValue = browser.First("strong span").GetText();

                // modify protected data
                browser.Last("a").Click();
                browser.Wait(750);

                // make sure it happened
                browser.First("strong span").CheckIfInnerTextEquals("hello");

                // try to do postback
                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                browser.Wait(500);
                browser.Click("input[type=button]");
                browser.Wait(750);

                // verify that the original value was restored
                browser.First("strong span").CheckIfInnerTextEquals(originalValue);
            });
        }

        private void CheckMessage(string messageDataUi, string originalText, string changedText, string afterPostBackText)
        {
            RunComplexViewModelProtectionTest(browser =>
            {
                var message = browser.Single(messageDataUi, this.SelectByDataUi);
                message.CheckIfTextEquals(originalText);
                browser.Single($"change-{messageDataUi}", this.SelectByDataUi).Click().Wait();

                message = browser.Single(messageDataUi, this.SelectByDataUi);
                message.CheckIfTextEquals(changedText);

            },
            browser => 
            {
                var message = browser.Single(messageDataUi, this.SelectByDataUi);
                message.CheckIfTextEquals(afterPostBackText);
            });
        }

        private void CheckRadioButtonsState(IBrowserWrapperFluentApi browser, RadioButtonValues selectedColor)
        {
            var radios = new List<IElementWrapper>();
            radios.Add(browser.Single("radio-red", this.SelectByDataUi));
            radios.Add(browser.Single("radio-green", this.SelectByDataUi));
            radios.Add(browser.Single("radio-blue", this.SelectByDataUi));
            var selectedColorElement = browser.Single("selected-color", this.SelectByDataUi);

            int checkedRadioIndex = (int)selectedColor;
            radios[checkedRadioIndex].CheckIfIsChecked();
            radios.RemoveAt(checkedRadioIndex);
            radios.ForEach(r => r.CheckIfIsNotChecked());

            selectedColorElement.CheckIfTextEquals(selectedColor.ToString().ToLower());
        }
        private void RunComplexViewModelProtectionTest(Action<IBrowserWrapperFluentApi> beforePostback, Action<IBrowserWrapperFluentApi> afterPostback)
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ViewModelProtection_ComplexViewModelProtection);

                beforePostback(browser);
                browser.Single("postback", this.SelectByDataUi).Click();
                browser.Wait(500);
                afterPostback(browser);
            });
        }
    }
}