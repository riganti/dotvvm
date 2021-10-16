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
    public class CommandActionFilterTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_CommandActionFilter_CallbacksReceivedTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_CommandActionFilter_CommandActionFilter);
                browser.WaitUntilDotvvmInited();

                browser.Click("input[type=button]");
                AssertUI.InnerText(browser.First("span"), s => s.Contains("SUCCESS"), "OnCommandExecutingAsync or OnCommandExecuted was not called!");
            });
        }

        public CommandActionFilterTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
