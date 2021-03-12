using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class JavascriptEventsTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_JavascriptEvents_JavascriptEvents()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptEvents_JavascriptEvents);

                // init alert
                
                AssertUI.AlertTextEquals(browser, "init");
                browser.ConfirmAlert();

                // postback alerts
                browser.ElementAt("input[type=button]", 0).Click();

                AssertUI.AlertTextEquals(browser, "beforePostback");
                browser.ConfirmAlert();
                

                AssertUI.AlertTextEquals(browser, "afterPostback");
                browser.ConfirmAlert();

                // error alerts
                browser.ElementAt("input[type=button]", 1).Click();

                AssertUI.AlertTextEquals(browser, "beforePostback");
                browser.ConfirmAlert();
                

                AssertUI.AlertTextEquals(browser, "afterPostback");
                browser.ConfirmAlert();
                

                AssertUI.AlertTextEquals(browser, "custom error handler");
                browser.ConfirmAlert();
            });
        }

        public JavascriptEventsTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
