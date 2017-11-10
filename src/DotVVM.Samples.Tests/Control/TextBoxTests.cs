
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing;
using System;
using System.Globalization;
using DotVVM.Testing.Abstractions;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class TextBoxTests : AppSeleniumTest
    {
        [TestMethod]
        public void Control_TextBox_FormatDoubleProperty()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_TextBox_TextBox_FormatDoubleProperty);

                browser.Single("[data-ui='textBox']").CheckIfTextEquals("0.00");
                browser.Single("[data-ui='button']").Click();
                browser.Wait(500);

                browser.Single("[data-ui='textBox']").CheckIfTextEquals("10.50");
            });
        }

        [TestMethod]
        public void Control_TextBox_IntBoundTextBox()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_TextBox_IntBoundTextBox);

                browser.ElementAt("input", 0).SendKeys("hello");
                browser.ElementAt("input[type=button]", 0).Click();
                browser.Wait();

                browser.ElementAt("input", 0).CheckIfInnerTextEquals("");
                browser.ElementAt("span", 0).CheckIfInnerTextEquals("0");
            });
        }

        [TestMethod]
        public void Control_TextBox_SimpleDateBox()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_TextBox_SimpleDateBox);

                var now = DateTime.Now;

                var typeText = browser.Single("[data-ui='type-text']").GetText();
                var typeTextDateTime = DateTime.Parse(typeText, DateTimeFormatInfo.InvariantInfo);
                Assert.AreEqual(now.ToShortDateString(), typeTextDateTime.ToShortDateString());

                var customFormat = browser.Single("[data-ui='custom-format']").GetText();
                Assert.AreEqual(customFormat, now.ToString("dd-MM-yy"));

                browser.Single("[data-ui='fill-name-button']").Click();
                browser.Single("[data-ui='name-of-day']")
                .CheckIfTextEquals(now.DayOfWeek.ToString());
            });
        }

        [TestMethod]
        public void Control_TextBox_TextBox()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_TextBox_TextBox);

                browser.First("#TextBox1").CheckTagName("input");
                browser.First("#TextBox2").CheckTagName("input");
                browser.First("#TextArea1").CheckTagName("textarea");
                browser.First("#TextArea2").CheckTagName("textarea");
            });
        }

        [TestMethod]
        public void Control_TextBox_TextBox_Format()
        {
            Control_TextBox_StringFormat_core(SamplesRouteUrls.ControlSamples_TextBox_TextBox_Format);
        }

        [TestMethod]
        public void Control_TextBox_TextBox_Format_Binding()
        {
            Control_TextBox_StringFormat_core(SamplesRouteUrls.ControlSamples_TextBox_TextBox_Format_Binding);
        }

        [TestMethod]
        public void Control_TextBox_SelectAllOnFocus()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_TextBox_SelectAllOnFocus);

                CheckSelectAllOnFocus(browser, "hardcoded");
                CheckSelectAllOnFocus(browser, "bound", false);
                browser.Single("button", this.SelectByDataUi).Click();
                CheckSelectAllOnFocus(browser, "bound", true);
            });
        }

        private void CheckSelectAllOnFocus(IBrowserWrapperFluentApi browser, string textBoxDataUi, bool isSelectAllOnFocusTrue = true)
        {
            var textBox = browser.Single(textBoxDataUi, this.SelectByDataUi);
            textBox.Click();
            var selectedText = (string)browser.GetJavaScriptExecutor().ExecuteScript("return window.getSelection().toString();");
            var expectedText = isSelectAllOnFocusTrue ? "Testing text" : "";
            Assert.AreEqual(expectedText, selectedText);
        }

        private void Control_TextBox_StringFormat_core(string url)
        {
            RunInAllBrowsers(browser =>
            {
                void checkForLanguage(string language)
                {
                    var culture = new CultureInfo(language);
                    var dateResult1 = browser.First("#date-result1").GetText();
                    var dateResult2 = browser.First("#date-result2").GetText();
                    var dateResult3 = browser.First("#date-result3").GetText();

                    var dateTextBox = browser.First("#dateTextbox").CheckAttribute("value", dateResult1);
                    var dateText = browser.First("#DateValueText").CheckIfInnerTextEquals(new DateTime(2015, 12, 27).ToString("G", culture));

                    var numberTextbox = browser.First("#numberTextbox").CheckAttribute("value", 123.1235.ToString(culture));
                    var numberValueText = browser.First("#numberValueText").CheckIfInnerTextEquals(123.123456789.ToString(culture));

                    //write new valid values
                    dateTextBox.Clear().SendKeys(dateResult2);
                    numberTextbox.Clear().SendKeys(2000.ToString("n0", culture));
                    dateTextBox.Click().Wait();

                    //check new values
                    dateText.CheckIfInnerTextEquals(new DateTime(2018, 12, 27).ToString("G", culture));
                    numberValueText.CheckIfInnerTextEquals(2000.ToString(culture));

                    numberTextbox.CheckAttribute("value", 2000.ToString("n4", culture));
                    dateTextBox.CheckAttribute("value", dateResult2);

                    //write invalid values
                    dateTextBox.Clear().SendKeys("dsasdasd");
                    numberTextbox.Clear().SendKeys("000//*a");
                    dateTextBox.Click();

                    //check invalid values
                    dateText.CheckIfInnerTextEquals("");
                    numberValueText.CheckIfInnerTextEquals("");

                    numberTextbox.CheckAttribute("value", "000//*a");
                    dateTextBox.CheckAttribute("value", "dsasdasd");

                    //write new valid values
                    dateTextBox.Clear().SendKeys(new DateTime(2018, 1, 1).ToString("d", culture));
                    numberTextbox.Clear().SendKeys(1000.550277.ToString(culture));
                    dateTextBox.Click().Wait();

                    //check new values
                    dateText.CheckIfInnerTextEquals(new DateTime(2018, 1, 1).ToString("G", culture));
                    numberValueText.CheckIfInnerTextEquals(1000.550277.ToString(culture));

                    numberTextbox.CheckAttribute("value", 1000.550277.ToString("n4", culture));
                    dateTextBox.CheckAttribute("value", dateResult3);
                };

                //en-US
                browser.NavigateToUrl(url);
                checkForLanguage("en-US");

                //cs-CZ | reload
                browser.First("#czech").Click();
                checkForLanguage("cs-CZ");
            });
        }
    }
}