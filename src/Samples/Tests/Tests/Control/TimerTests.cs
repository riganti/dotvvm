using System;
using DotVVM.Samples.Tests.Base;
using Riganti.Selenium.Core;
using DotVVM.Testing.Abstractions;
using Xunit;
using Xunit.Abstractions;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium;
using Riganti.Selenium.Core.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class TimerTests : AppSeleniumTest
    {
        public TimerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Control_Timer_Timer_Timer1()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Timer_Timer);

                var value = browser.Single("[data-ui=value1]");

                // ensure the first timer is running
                AssertEqualsWithTolerance(0, int.Parse(value.GetInnerText()), 1);
                browser.Wait(3000);
                AssertEqualsWithTolerance(3, int.Parse(value.GetInnerText()), 1);
                browser.Wait(3000);
                AssertEqualsWithTolerance(6, int.Parse(value.GetInnerText()), 1);

                // stop the first timer
                browser.Single("[data-ui=enabled1]").Click();
                browser.Wait(3000);
                AssertEqualsWithTolerance(6, int.Parse(value.GetInnerText()), 1);
                browser.Wait(3000);
                AssertEqualsWithTolerance(6, int.Parse(value.GetInnerText()), 1);

                // restart the timer
                browser.Single("[data-ui=enabled1]").Click();
                var restartValue = int.Parse(value.GetInnerText());
                browser.Wait(3000);
                AssertEqualsWithTolerance(restartValue + 3, int.Parse(value.GetInnerText()), 1);
                browser.Wait(3000);
                AssertEqualsWithTolerance(restartValue + 6, int.Parse(value.GetInnerText()), 1);
            });
        }

        [Fact]
        public void Control_Timer_Timer_Timer2()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Timer_Timer);

                var value = browser.Single("[data-ui=value2]");

                // ensure the timer is not running
                AssertEqualsWithTolerance(0, int.Parse(value.GetInnerText()), 1);
                browser.Wait(3000);
                AssertEqualsWithTolerance(0, int.Parse(value.GetInnerText()), 1);
                browser.Wait(3000);
                AssertEqualsWithTolerance(0, int.Parse(value.GetInnerText()), 1);

                // start the second timer
                browser.Single("[data-ui=enabled2]").Click();
                browser.Wait(4000);
                AssertEqualsWithTolerance(2, int.Parse(value.GetInnerText()), 1);
                browser.Wait(4000);
                AssertEqualsWithTolerance(4, int.Parse(value.GetInnerText()), 1);

                // stop the second timer
                browser.Single("[data-ui=enabled2]").Click();
                browser.Wait(4000);
                AssertEqualsWithTolerance(4, int.Parse(value.GetInnerText()), 1);
                browser.Wait(4000);
                AssertEqualsWithTolerance(4, int.Parse(value.GetInnerText()), 1);
            });
        }


        [Fact]
        public void Control_Timer_Timer_Timer3()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Timer_Timer);

                var value = browser.Single("[data-ui=value3]");

                // ensure the timer is running
                AssertEqualsWithTolerance(0, int.Parse(value.GetInnerText()), 1);
                browser.Wait(3000);
                AssertEqualsWithTolerance(1, int.Parse(value.GetInnerText()), 1);
                browser.Wait(3000);
                AssertEqualsWithTolerance(2, int.Parse(value.GetInnerText()), 1);
            });
        }

        [Fact]
        public void Control_Timer_LongCommand()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Timer_LongCommand);

                var value = browser.Single(".result");

                // ensure the new command does not start before the old finishes
                AssertEqualsWithTolerance(0, int.Parse(value.GetInnerText()), 1);
                browser.Wait(4000);

                AssertEqualsWithTolerance(1, int.Parse(value.GetInnerText()), 1);
                browser.Wait(4000);

                AssertEqualsWithTolerance(2, int.Parse(value.GetInnerText()), 1);
                browser.Wait(4000);

                AssertEqualsWithTolerance(3, int.Parse(value.GetInnerText()), 1);
            });
        }


        [Fact]
        public void Control_Timer_Removal()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Timer_Removal);

                var value = browser.Single(".result");

                // ensure the timer works
                AssertEqualsWithTolerance(0, int.Parse(value.GetInnerText()), 1);
                browser.Wait(1000);

                AssertEqualsWithTolerance(1, int.Parse(value.GetInnerText()), 1);
                browser.Wait(1000);

                // disable the timer
                browser.Single("disabled", SelectByDataUi).Click();
                AssertEqualsWithTolerance(2, int.Parse(value.GetInnerText()), 1);
                browser.Wait(2000);
                AssertEqualsWithTolerance(2, int.Parse(value.GetInnerText()), 1);

                // remove the timer from DOM
                browser.Single("remove", SelectByDataUi).Click();
                AssertEqualsWithTolerance(2, int.Parse(value.GetInnerText()), 1);
                browser.Wait(2000);
                AssertEqualsWithTolerance(2, int.Parse(value.GetInnerText()), 1);

                // reenable the timer
                browser.Single("disabled", SelectByDataUi).Click();

                // make sure it hasn't started
                AssertEqualsWithTolerance(2, int.Parse(value.GetInnerText()), 1);
                browser.Wait(2000);
                AssertEqualsWithTolerance(2, int.Parse(value.GetInnerText()), 1);
            });
        }

        private static void AssertEqualsWithTolerance(int expected, int actual, int tolerance)
            => Assert.True(Math.Abs(expected - actual) <= tolerance,
                $"Expected timer value {expected} with tolerance {tolerance} (range {expected - tolerance} to {expected + tolerance}), actual {actual}.");
    }
}
