using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
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
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_TextBox_TextBox_Format);

                //en-US
                var dateResult1 = browser.First("#date-result1").GetText();
                var dateResult2 = browser.First("#date-result2").GetText();
                var dateResult3 = browser.First("#date-result3").GetText();

                var dateTextBox = browser.First("#dateTextbox").CheckAttribute("value", dateResult1);
                var dateText = browser.First("#DateValueText").CheckIfInnerTextEquals("12/27/2015 12:00:00 AM");

                var numberTextbox = browser.First("#numberTextbox").CheckAttribute("value", "123.1235");
                var numberValueText = browser.First("#numberValueText").CheckIfInnerTextEquals("123.123456789");

                //write new valid values 
                dateTextBox.Clear().SendKeys(dateResult2);
                numberTextbox.Clear().SendKeys("2,000");
                dateTextBox.Click().Wait();


                //check new values
                dateText.CheckIfInnerTextEquals("12/27/2018 12:00:00 AM");
                numberValueText.CheckIfInnerTextEquals("2000");

                numberTextbox.CheckAttribute("value", "2,000.0000");
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
                dateTextBox.Clear().SendKeys("1/1/2018");
                numberTextbox.Clear().SendKeys("1000.550277");
                dateTextBox.Click().Wait();


                //check new values 
                dateText.CheckIfInnerTextEquals("1/1/2018 12:00:00 AM");
                numberValueText.CheckIfInnerTextEquals("1000.550277");

                numberTextbox.CheckAttribute("value", "1,000.5503");
                dateTextBox.CheckAttribute("value", dateResult3);

                //cs-CZ | reload
                browser.First("#czech").Click();
                dateTextBox = browser.First("#dateTextbox");
                dateText = browser.First("#DateValueText");
                numberTextbox = browser.First("#numberTextbox");
                numberValueText = browser.First("#numberValueText");

                dateResult1 = browser.First("#date-result1").GetText();
                dateResult2 = browser.First("#date-result2").GetText();
                dateResult3 = browser.First("#date-result3").GetText();

                dateTextBox.CheckAttribute("value", dateResult1);
                dateText.CheckIfInnerTextEquals("27.12.2015 0:00:00");

                numberTextbox.CheckAttribute("value", "123,1235");
                numberValueText.CheckIfInnerTextEquals("123.123456789");

                //write new valid values 
                dateTextBox.Clear().SendKeys(dateResult2);
                numberTextbox.Clear().SendKeys("2,000");
                dateTextBox.Click().Wait();


                //check new values 
                dateText.CheckIfInnerTextEquals("27.12.2018 0:00:00");
                numberValueText.CheckIfInnerTextEquals("2");

                numberTextbox.CheckAttribute("value", "2,0000");
                dateTextBox.CheckAttribute("value", dateResult2);

                //write invalid values
                dateTextBox.Clear().SendKeys("dsasdasd");
                numberTextbox.Clear().SendKeys("000//a");
                dateTextBox.Click();


                //check invalid values
                dateText.CheckIfInnerTextEquals("");
                numberValueText.CheckIfInnerTextEquals("");

                numberTextbox.CheckAttribute("value", "000//a");
                dateTextBox.CheckAttribute("value", "dsasdasd");


                //write new valid values 
                dateTextBox.Clear().SendKeys(dateResult3);
                numberTextbox.Clear().SendKeys("1000.550277");
                dateTextBox.Click().Wait();


                //check new values 
                dateText.CheckIfInnerTextEquals("01.01.2018 0:00:00");
                numberValueText.CheckIfInnerTextEquals("1000.550277");

                numberTextbox.CheckAttribute("value", "1 000,5503");
                dateTextBox.CheckAttribute("value", dateResult3);
            });
        }
    }
}