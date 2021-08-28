using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class StringInterpolationTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_StringInterpolation_SpecialCharacterTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StringInterpolation_StringInterpolation);
                var text1 = browser.Single("special-char1", SelectByDataUi);
                var text2 = browser.Single("special-char2", SelectByDataUi);

                AssertUI.TextEquals(text1, "He asked, \"Is your name Mark?\", but didn't wait for a reply :-{");
                AssertUI.TextEquals(text2, "Mark is 24 years old.");
            });
        }
        [Fact]
        public void Feature_StringInterpolation_StandardNumericFormatTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StringInterpolation_StringInterpolation);
                var text1 = browser.Single("standard-numeric-format1", SelectByDataUi);
                var text2 = browser.Single("standard-numeric-format2", SelectByDataUi);
                var text3 = browser.Single("standard-numeric-format3", SelectByDataUi);
                var text4 = browser.Single("standard-numeric-format4", SelectByDataUi);
                var text5 = browser.Single("standard-numeric-format5", SelectByDataUi);
                var text6 = browser.Single("standard-numeric-format6", SelectByDataUi);

                AssertUI.TextEquals(text1, "No format: -1508");
                AssertUI.TextEquals(text2, "C2 format: $15.09");
                AssertUI.TextEquals(text3, "G1 format: 15.0896");
                AssertUI.TextEquals(text4, "N format: -1,508.00");
                AssertUI.TextEquals(text5, "D8 format: -00001508");
                AssertUI.TextEquals(text6, "P format: 1,508 %");
            });
        }
        [Fact]
        public void Feature_StringInterpolation_CustomNumericFormatTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StringInterpolation_StringInterpolation);
                var text1 = browser.Single("custom-numeric-format1", SelectByDataUi);
                var text2 = browser.Single("custom-numeric-format2", SelectByDataUi);
                var text3 = browser.Single("custom-numeric-format3", SelectByDataUi);

                AssertUI.TextEquals(text1, "15.0896 (#####.#) -> 15.1");
                AssertUI.TextEquals(text2, "15.0896 (00000.0) -> 00015.1");
                AssertUI.TextEquals(text3, "15.0896 (#####) -> 15");
            });
        }
        [Fact]
        public void Feature_StringInterpolation_StandardDateFormatTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StringInterpolation_StringInterpolation);
                var text1 = browser.Single("date-format1", SelectByDataUi);
                var text2 = browser.Single("date-format2", SelectByDataUi);
                var text3 = browser.Single("date-format3", SelectByDataUi);
                var text4 = browser.Single("date-format4", SelectByDataUi);
                var text5 = browser.Single("date-format5", SelectByDataUi);
                var text6 = browser.Single("date-format6", SelectByDataUi);
                var text7 = browser.Single("date-format7", SelectByDataUi);

                AssertUI.TextEquals(text1, "No format: 2016-07-15T03:15:00.0000000");
                AssertUI.TextEquals(text2, "D format: Friday, July 15, 2016 |X| d format: 7/15/2016");
                AssertUI.TextEquals(text3, "F format: Friday, July 15, 2016 3:15:00 AM |X| f format: Friday, July 15, 2016 3:15 AM");
                AssertUI.TextEquals(text4, "G format: 7/15/2016 3:15:00 AM |X| g format: 7/15/2016 3:15 AM");
                AssertUI.TextEquals(text5, "M format: July 15");
                AssertUI.TextEquals(text6, "T format: 3:15:00 AM |X| t format: 3:15 AM");
                AssertUI.TextEquals(text7, "Y format: 2016 July");
            });
        }
        [Fact]
        public void Feature_StringInterpolation_CustomDateFormatTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StringInterpolation_StringInterpolation);
                var text1 = browser.Single("custom-date-format1", SelectByDataUi);
                var text2 = browser.Single("custom-date-format2", SelectByDataUi);
                var text3 = browser.Single("custom-date-format3", SelectByDataUi);

                AssertUI.TextEquals(text1, "dd MMM yyyy hh:mm tt PST format: 15 Jul 2016 03:15 AM PST");
                AssertUI.TextEquals(text2, "ddd dd MM yyyy format: Fri 15 07 2016");
                AssertUI.TextEquals(text3, "dddd dd MMMM yyyy format: Friday 15 July 2016");
            });
        }
        public StringInterpolationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
