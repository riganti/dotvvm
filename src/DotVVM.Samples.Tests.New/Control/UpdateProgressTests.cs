using Riganti.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.New;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core.Abstractions.Exceptions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class UpdateProgressTests : AppSeleniumTest
    {

        [Fact]
        public void Control_UpdateProgress_UpdateProgress()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgress);
                browser.Wait();

                // click the button and verify that the progress appears and disappears again
           AssertUI.IsNotDisplayed(browser.First(".update-progress"));
                browser.ElementAt("input[type=button]", 0).Click();
                AssertUI.IsDisplayed(browser.First(".update-progress"));
                browser.Wait(3000);
                AssertUI.IsNotDisplayed(browser.First(".update-progress"));

                // click the second button and verify that the progress appears and disappears again
                AssertUI.IsNotDisplayed(browser.First(".update-progress"));
                browser.ElementAt("input[type=button]", 1).Click();
                browser.Wait(1000);
                AssertUI.IsNotDisplayed(browser.First(".update-progress"));
            });
        }

        [Fact]
        public void Control_UpdateProgress_UpdateProgressDelayLongTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressDelay);
                browser.Wait();

                // click the button with long test and verify that the progress appears and disappears again
                AssertUI.IsNotDisplayed(browser.First(".update-progress"));
                browser.First(".long-test").Click();

                //wait for the progress to be shown
                browser.WaitFor(() =>
                {
                    AssertUI.IsDisplayed(browser.First(".update-progress"));
                }, 3000);

                //verify that the progress disappears 
                browser.WaitFor(() =>
                {
                    AssertUI.IsNotDisplayed(browser.First(".update-progress"));
                }, 2000);
            });
        }

        [Fact]
        public void Control_UpdateProgress_UpdateProgressDelayShortTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressDelay);
                browser.Wait();

                // click the second button with short test and verify that the progress does not appear
                AssertUI.IsNotDisplayed(browser.First(".update-progress"));
                browser.First(".short-test").Click();

                browser.WaitFor(() => AssertUI.IsNotDisplayed(browser.First(".update-progress")), 3000);

            });
        }

        [Fact]
        public void Control_UpdateProgress_UpdateProgressDelayInterruptTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressDelay);
                browser.Wait();
                var updateProgressControl = browser.First(".update-progress");

                // click the second button with short test and verify that the progress does not appear
                AssertUI.IsNotDisplayed(updateProgressControl);
                browser.First(".long-test").Click();
                //waiting for the update progress to show up
                browser.WaitFor(() =>
                {
                    AssertUI.IsDisplayed(updateProgressControl);
                }, 3000);

                //interrupting first update progress (it should not be displayed and the timer should reset)
                browser.First(".long-test").Click();
                AssertUI.IsNotDisplayed(updateProgressControl);
                //waiting for the update progress to show up again
                browser.WaitFor(() =>
                {
                    AssertUI.IsDisplayed(updateProgressControl);
                }, 3000);
            });
        }

        public UpdateProgressTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
