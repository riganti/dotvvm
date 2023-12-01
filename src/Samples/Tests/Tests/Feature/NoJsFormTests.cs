using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class NoJsFormTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_NoJsForm_NoJsForm()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_NoJsForm_NoJsForm);

                browser.SendKeys("#input1", "Q");
                browser.Click("#submit1");
                AssertUI.InnerTextEquals(browser.First("#result1"), "Q");

                browser.SendKeys("#input2", "W");
                browser.Click("#submit2");
                AssertUI.InnerTextEquals(browser.First("#result2"), "W");
            });
        }

        public NoJsFormTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
