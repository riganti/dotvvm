using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core.Abstractions.Exceptions;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class UpdateProgressTests : AppSeleniumTest
    {

        [TestMethod]
        public void Control_UpdateProgress_UpdateProgress()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgress);
                browser.Wait();

                // click the button and verify that the progress appears and disappears again
                browser.First(".update-progress").CheckIfIsNotDisplayed();
                browser.ElementAt("input[type=button]", 0).Click();
                browser.First(".update-progress").CheckIfIsDisplayed();
                browser.Wait(3000);
                browser.First(".update-progress").CheckIfIsNotDisplayed();

                // click the second button and verify that the progress appears and disappears again
                browser.First(".update-progress").CheckIfIsNotDisplayed();
                browser.ElementAt("input[type=button]", 1).Click();
                browser.Wait(1000);
                browser.First(".update-progress").CheckIfIsNotDisplayed();
            });
        }

        [TestMethod]
        public void Control_UpdateProgress_UpdateProgressDelayLongTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressDelay);
                browser.Wait();

                // click the button with long test and verify that the progress appears and disappears again
                browser.First(".update-progress").CheckIfIsNotDisplayed();
                browser.First(".long-test").Click();

                //wait for the progress to be shown
                browser.WaitFor(() =>
                {
                    browser.First(".update-progress").CheckIfIsDisplayed();
                }, 3000);

                //verify that the progress disappears 
                browser.WaitFor(() =>
                {
                    browser.First(".update-progress").CheckIfIsNotDisplayed();
                }, 2000);
            });
        }

        [TestMethod]
        public void Control_UpdateProgress_UpdateProgressDelayShortTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressDelay);
                browser.Wait();

                // click the second button with short test and verify that the progress does not appear
                browser.First(".update-progress").CheckIfIsNotDisplayed();
                browser.First(".short-test").Click();

                browser.WaitFor(() => browser.First(".update-progress").CheckIfIsNotDisplayed(), 3000);

            });
        }

        [TestMethod]
        public void Control_UpdateProgress_UpdateProgressDelayInterruptTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressDelay);
                browser.Wait();

                // click the second button with short test and verify that the progress does not appear
                browser.First(".update-progress").CheckIfIsNotDisplayed();
                browser.First(".long-test").Click();
                //waiting for the update progress to show up
                browser.WaitFor(() =>
                {
                    browser.First(".update-progress").CheckIfIsDisplayed();
                }, 3000);

                //interrupting first update progress (it should not be displayed and the timer should reset)
                browser.First(".long-test").Click();
                browser.First(".update-progress").CheckIfIsNotDisplayed();
                //waiting for the update progress to show up again
                browser.WaitFor(() =>
                {
                    browser.First(".update-progress").CheckIfIsDisplayed();
                }, 3000);
            });
        }
    }
}
