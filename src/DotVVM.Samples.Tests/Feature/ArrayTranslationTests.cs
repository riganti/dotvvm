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
    public class ArrayTranslationTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_ArrayTranslation_SetItem()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_ArrayTranslation);

                // Set list index
                var indexTextbox = browser.FindElements("input[data-ui=index]");
                indexTextbox.FirstOrDefault().Clear().SendKeys("0");
                // Set list value
                var valueTextbox = browser.FindElements("input[data-ui=value]");
                valueTextbox.FirstOrDefault().Clear().SendKeys("Hello world");
                // Change element
                var setButton = browser.FindElements("input[data-ui=set]");
                setButton.FirstOrDefault().Click();

                var spans = browser.FindElements("span");
                AssertUI.TextEquals(spans.ElementAt(0), "INDEX: \"0\"");
                AssertUI.TextEquals(spans.ElementAt(1), "VALUE: \"Hello world\"");
            });
        }

        public ArrayTranslationTests(ITestOutputHelper output) : base(output)
        {

        }
    }
}
