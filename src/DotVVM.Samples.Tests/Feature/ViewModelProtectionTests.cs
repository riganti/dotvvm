using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;
using System;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ViewModelProtectionTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_ViewModelProtection_ComplexViewModelProtection_NestedProtection()
        {
            RunComplexViewModelProtectionTest(browser =>
            {
                var songTitle = browser.Single("song-title", this.SelectByDataUi);
                var songAuthor = browser.Single("song-author", this.SelectByDataUi);
                songTitle.CheckIfTextEquals("A Song");
                songAuthor.CheckIfTextEquals("John Smith");
                browser.Single("change-song-title", this.SelectByDataUi).Click();
                browser.Single("change-song-author", this.SelectByDataUi).Click();
                songTitle.CheckIfTextEquals("New Title");
                songAuthor.CheckIfTextEquals("Joe Pirate");
            }, browser =>
            {
                var songTitle = browser.Single("song-title", this.SelectByDataUi);
                var songAuthor = browser.Single("song-author", this.SelectByDataUi);
                songTitle.CheckIfTextEquals("New Title");
                songAuthor.CheckIfTextEquals("John Smith");
            });
        }

        [TestMethod]
        public void Feature_ViewModelProtection_ComplexViewModelProtection_ServerToClient()
        {
            // this test is here because it's necessary to test if the BindAttribute and ProtectAttribute can both work in the same viewModel
            RunComplexViewModelProtectionTest(browser =>
            {
                var message = browser.Single("message", this.SelectByDataUi);
                message.CheckIfTextEquals("Sample text");
                browser.Single("change-message", this.SelectByDataUi).Click();
                message.CheckIfTextEquals("A different message");
            }, browser =>
            {
                var message = browser.Single("message", this.SelectByDataUi);
                message.CheckIfTextEquals("Sample text");
            });
        }

        [TestMethod]
        public void Feature_ViewModelProtection_ComplexViewModelProtection_SignedList()
        {
            RunComplexViewModelProtectionTest(browser =>
            {
                var repeater = browser.Single("repeater-seasons", this.SelectByDataUi);
                repeater.CheckIfTextEquals("Spring, Summer, Autumn, Winter,");
                browser.Single("change-seasons", this.SelectByDataUi).Click();
                repeater.CheckIfInnerTextEquals("");
            }, browser =>
            {
                var repeater = browser.Single("repeater-seasons", this.SelectByDataUi);
                repeater.CheckIfTextEquals("Spring, Summer, Autumn, Winter,");
            });
        }

        [TestMethod]
        public void Feature_ViewModelProtection_ComplexViewModelProtection_SignedString()
        {
            RunComplexViewModelProtectionTest(browser =>
            {
                CheckRadioButtonsInitialState(browser);
                browser.Single("change-color", this.SelectByDataUi).Click();
                CheckRadioButtonsChangedState(browser);
            }, CheckRadioButtonsInitialState);
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

        private void CheckRadioButtonsChangedState(BrowserWrapper browser)
        {
            var radioRed = browser.Single("radio-red", this.SelectByDataUi);
            var radioGreen = browser.Single("radio-green", this.SelectByDataUi);
            var radioBlue = browser.Single("radio-blue", this.SelectByDataUi);
            var selectedColor = browser.Single("selected-color", this.SelectByDataUi);

            radioRed.CheckIfIsNotChecked();
            radioGreen.CheckIfIsChecked();
            radioBlue.CheckIfIsNotChecked();
            selectedColor.CheckIfTextEquals("green");
        }

        private void CheckRadioButtonsInitialState(BrowserWrapper browser)
        {
            var radioRed = browser.Single("radio-red", this.SelectByDataUi);
            var radioGreen = browser.Single("radio-green", this.SelectByDataUi);
            var radioBlue = browser.Single("radio-blue", this.SelectByDataUi);
            var selectedColor = browser.Single("selected-color", this.SelectByDataUi);

            radioRed.CheckIfIsChecked();
            radioGreen.CheckIfIsNotChecked();
            radioBlue.CheckIfIsNotChecked();
            selectedColor.CheckIfTextEquals("red");
        }

        private void RunComplexViewModelProtectionTest(Action<BrowserWrapper> beforePostback, Action<BrowserWrapper> afterPostback)
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