using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class TextBoxTests : SeleniumTestBase
    {
        [TestMethod]
        public void Control_TextBox()
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
        public void Control_TextBox_StringFormat()
        {
            RunInAllBrowsers(browser =>
            {
                Action<string> checkForLanguage = (language) =>
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
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_TextBox_TextBox_Format);
                checkForLanguage("en-US");

                //cs-CZ | reload
                browser.First("#czech").Click();
                checkForLanguage("cs-CZ");
            });
        }
    }
}