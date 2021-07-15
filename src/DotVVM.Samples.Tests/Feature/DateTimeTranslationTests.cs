using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class DateTimeTranslationTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_DateTime_PropertyTranslations()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_DateTimeTranslations);

                var stringDateTime = "6/28/2021 3:28:31 PM";

                var textbox = browser.FindElements("input[data-ui=textbox]").FirstOrDefault();
                textbox.Clear().SendKeys(stringDateTime).SendEnterKey();

                var spanYear = browser.FindElements("span[data-ui=year]").FirstOrDefault();
                AssertUI.TextEquals(spanYear, "2021");
                var spanMonth = browser.FindElements("span[data-ui=month]").FirstOrDefault();
                AssertUI.TextEquals(spanMonth, "6");
                var spanDay = browser.FindElements("span[data-ui=day]").FirstOrDefault();
                AssertUI.TextEquals(spanDay, "28");
                var spanHour = browser.FindElements("span[data-ui=hour]").FirstOrDefault();
                AssertUI.TextEquals(spanHour, "15");
                var spanMinute = browser.FindElements("span[data-ui=minute]").FirstOrDefault();
                AssertUI.TextEquals(spanMinute, "28");
                var spanSecond = browser.FindElements("span[data-ui=second]").FirstOrDefault();
                AssertUI.TextEquals(spanSecond, "31");
                var spanMillisecond = browser.FindElements("span[data-ui=millisecond]").FirstOrDefault();
                AssertUI.TextEquals(spanMillisecond, "0");
            });
        }

        public DateTimeTranslationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
