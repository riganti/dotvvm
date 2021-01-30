using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Xunit;
using Xunit.Abstractions;
using Riganti.Selenium.DotVVM;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class CommandResultTests : AppSeleniumTest
    {
        public CommandResultTests(ITestOutputHelper output) : base(output)
        {
        }
        [Fact]
        public void SimpleExceptionFilterTest()
        {
            base.RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_CustomResponseProperties_SimpleExceptionFilter);
                browser.WaitUntilDotvvmInited();

                TestResponse(browser, "staticCommand");
                TestResponse(browser, "command");

                TestResponse(browser, "asyncStaticCommand");
                TestResponse(browser, "asyncCommand");

                TestResponse(browser, "staticCommandResult");
                TestResponse(browser, "commandResult");

                TestResponse(browser, "asyncStaticCommandResult");
                TestResponse(browser, "asyncCommandResult");
            });
        }

        private void TestResponse(IBrowserWrapper browser, string uiId)
        {
            var staticCommandButton = browser.First(uiId, SelectByDataUi);
            staticCommandButton.Click();

            browser.WaitFor(() => {
                var customDataSpan = browser.First("customProperties", SelectByDataUi);
                AssertUI.TextEquals(customDataSpan, "Hello there");
            }, 8000);

            var clearButton = browser.First("clear", SelectByDataUi);
            clearButton.Click();
        }
    }
}
