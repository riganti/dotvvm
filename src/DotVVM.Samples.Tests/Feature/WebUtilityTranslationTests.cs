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
    public class WebUtilityTranslationTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_WebUtility_UrlEncodeDecode()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_WebUtilityTranslations);
                var inputText = @"Encoding test ""&?<>";

                var textbox = browser.FindElements("input[data-ui=textbox]").FirstOrDefault();
                textbox.Clear().SendKeys(inputText).SendEnterKey();

                var spanEncoded = browser.FindElements("span[data-ui=encoded]").FirstOrDefault();
                AssertUI.TextEquals(spanEncoded, "Encoding%20test%20%22%26%3F%3C%3E");
                var spanDecoded = browser.FindElements("span[data-ui=decoded]").FirstOrDefault();
                AssertUI.TextEquals(spanDecoded, inputText);
            });
        }

        public WebUtilityTranslationTests(ITestOutputHelper output) : base(output)
        {
        }

    }
}
