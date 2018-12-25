using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class ActionFilterErrorHandlingTest : AppSeleniumTest
    {
        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ActionFilterErrorHandling_ActionFilterErrorHandling))]
        public void Feature_ActionFilterErrorHandling_ActionFilterErrorHandling_CommandException()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ActionFilterErrorHandling_ActionFilterErrorHandling);

                AssertUI.InnerTextEquals(browser.Single(".result"), "no error");

                browser.ElementAt("input[type=button]", 0).Click();
                browser.WaitForPostback();
                AssertUI.IsNotDisplayed(browser.Single("iframe"));
                AssertUI.InnerTextEquals(browser.Single(".result"), "error was handled");

                browser.ElementAt("input[type=button]", 1).Click();
                browser.WaitForPostback();
                AssertUI.IsDisplayed(browser.Single("iframe"));
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ActionFilterErrorHandling_ActionFilterPageErrorHandling))]
        public void Feature_ActionFilterErrorHandling_ActionFilterErrorHandling_PageException()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ActionFilterErrorHandling_ActionFilterPageErrorHandling);
                browser.Wait(1000);
                AssertUI.Url(browser, u => u.Contains("error500"));
            });
        }

        [Fact]
        public void Feature_ActionFilterErrorHandling_ActionFilterRedirect()
        {
            RunInAllBrowsers(browser => {
                // try the first button
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ActionFilterErrorHandling_ActionFilterRedirect);
                browser.Wait();
                AssertUI.Url(browser, u => !u.Contains("?redirected=true"));
                browser.ElementAt("input", 0).Click().Wait();
                AssertUI.Url(browser, u => u.Contains("?redirected=true"));

                // try the second button
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ActionFilterErrorHandling_ActionFilterRedirect);
                browser.Wait();
                AssertUI.Url(browser, u => !u.Contains("?redirected=true"));
                browser.ElementAt("input", 1).Click().Wait();
                AssertUI.Url(browser, u => u.Contains("?redirected=true"));
            });
        }

        public ActionFilterErrorHandlingTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}
