﻿using Riganti.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core.Abstractions.Exceptions;
using Xunit;
using Xunit.Abstractions;
using OpenQA.Selenium.Chrome;
using Riganti.Selenium.Core.Abstractions.Attributes;

namespace DotVVM.Samples.Tests.Control
{
    public class UpdateProgressTests : AppSeleniumTest
    {

        [Fact]
        public void Control_UpdateProgress_UpdateProgress()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgress);

                // click the button and verify that the progress appears and disappears again
                AssertUI.IsNotDisplayed(browser.First(".update-progress"));
                browser.ElementAt("input[type=button]", 0).Click();
                AssertUI.IsDisplayed(browser.First(".update-progress"));
                browser.Wait(3000);
                AssertUI.IsNotDisplayed(browser.First(".update-progress"));

                // click the second button and verify that the progress appears and disappears again
                AssertUI.IsNotDisplayed(browser.First(".update-progress"));
                browser.ElementAt("input[type=button]", 1).Click();
                AssertUI.IsNotDisplayed(browser.First(".update-progress"));
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressDelay))]
        public void Control_UpdateProgress_UpdateProgressDelayLongTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressDelay);

                // click the button with long test and verify that the progress appears and disappears again
                AssertUI.IsNotDisplayed(browser.First(".update-progress"));
                browser.First(".long-test").Click();

                //wait for the progress to be shown
                browser.WaitFor(() => {
                    AssertUI.IsDisplayed(browser.First(".update-progress"));
                }, 3000);

                //verify that the progress disappears 
                browser.WaitFor(() => {
                    AssertUI.IsNotDisplayed(browser.First(".update-progress"));
                }, 2000);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressDelay))]
        public void Control_UpdateProgress_UpdateProgressDelayShortTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressDelay);

                // click the second button with short test and verify that the progress does not appear
                AssertUI.IsNotDisplayed(browser.First(".update-progress"));
                browser.First(".short-test").Click();

                AssertUI.IsNotDisplayed(browser.First(".update-progress"));

            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressDelay))]
        public void Control_UpdateProgress_UpdateProgressDelayInterruptTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressDelay);
                 browser.Wait();
                var updateProgressControl = browser.First(".update-progress");

                // click the second button with short test and verify that the progress does not appear
                AssertUI.IsNotDisplayed(updateProgressControl);
                browser.First(".short-test").Click();
                //waiting for the update progress to show up
                browser.WaitFor(() => {
                    AssertUI.IsNotDisplayed(updateProgressControl);
                }, 3000);

                // click the first button with long test and verify that the progress does appear
                AssertUI.IsNotDisplayed(updateProgressControl);
                browser.First(".long-test").Click();
                //waiting for the update progress to show up
                browser.WaitFor(() => {
                    AssertUI.IsDisplayed(updateProgressControl);
                }, 3000);

                //interrupting first postback with another postback (it should still be displayed and wait to second postback end)
                AssertUI.IsDisplayed(updateProgressControl);
                browser.First(".long-test").Click();
                //update progress should be displayed during whole postback
                browser.Wait(2000);
                AssertUI.IsDisplayed(updateProgressControl);

                //update progress should disapear after postback end
                browser.WaitFor(() => {
                    AssertUI.IsNotDisplayed(updateProgressControl);
                }, 2000);
            });
        }

        [Fact]
        public void Control_UpdateProgress_UpdateProgressQueues()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressQueues);

                var button1 = browser.ElementAt("input[type=button]", 0);
                var button2 = browser.ElementAt("input[type=button]", 1);
                var button3 = browser.ElementAt("input[type=button]", 2);
                var buttonDefault = browser.ElementAt("input[type=button]", 3);
                var progress1 = browser.Single(".updateprogress-allqueues");
                var progress2 = browser.Single(".updateprogress-queue1");
                var progress3 = browser.Single(".updateprogress-queue12");
                var progress4 = browser.Single(".updateprogress-exclude1default");

                // first button
                AssertUI.IsNotDisplayed(progress1);
                AssertUI.IsNotDisplayed(progress2);
                AssertUI.IsNotDisplayed(progress3);
                AssertUI.IsNotDisplayed(progress4);
                button1.Click();
                AssertUI.IsDisplayed(progress1);
                AssertUI.IsDisplayed(progress2);
                AssertUI.IsDisplayed(progress3);
                AssertUI.IsNotDisplayed(progress4);
                browser.Wait(1500);

                // second button
                AssertUI.IsNotDisplayed(progress1);
                AssertUI.IsNotDisplayed(progress2);
                AssertUI.IsNotDisplayed(progress3);
                AssertUI.IsNotDisplayed(progress4);
                button2.Click();
                AssertUI.IsDisplayed(progress1);
                AssertUI.IsNotDisplayed(progress2);
                AssertUI.IsDisplayed(progress3);
                AssertUI.IsDisplayed(progress4);
                browser.Wait(1500);

                // third button
                AssertUI.IsNotDisplayed(progress1);
                AssertUI.IsNotDisplayed(progress2);
                AssertUI.IsNotDisplayed(progress3);
                AssertUI.IsNotDisplayed(progress4);
                button3.Click();
                AssertUI.IsDisplayed(progress1);
                AssertUI.IsNotDisplayed(progress2);
                AssertUI.IsNotDisplayed(progress3);
                AssertUI.IsDisplayed(progress4);
                browser.Wait(1500);

                // fourth button
                AssertUI.IsNotDisplayed(progress1);
                AssertUI.IsNotDisplayed(progress2);
                AssertUI.IsNotDisplayed(progress3);
                AssertUI.IsNotDisplayed(progress4);
                buttonDefault.Click();
                AssertUI.IsDisplayed(progress1);
                AssertUI.IsNotDisplayed(progress2);
                AssertUI.IsNotDisplayed(progress3);
                AssertUI.IsNotDisplayed(progress4);
                browser.Wait(1500);

                AssertUI.IsNotDisplayed(progress1);
                AssertUI.IsNotDisplayed(progress2);
                AssertUI.IsNotDisplayed(progress3);
                AssertUI.IsNotDisplayed(progress4);
            });
        }

        [Theory]
        [InlineData(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressRedirectSPA1)]
        [InlineData(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressRedirect1)]
        public void Control_UpdateProgress_SPA_Redirect(string route)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(route);

                var spaTextElement = browser.Single("text", SelectByDataUi);
                var goToSpa2Btn = browser.Single("btn-2", SelectByDataUi);
                var progress = browser.Single("progress", By.Id);

                AssertUI.InnerTextEquals(spaTextElement, "PAGE 1");
                AssertUI.IsNotDisplayed(progress);

                goToSpa2Btn.Click();

                var sw = new Stopwatch();
                sw.Start();
                while (sw.ElapsedMilliseconds < 2100) // action should take only 2000ms
                {
                    string spaText;
                    try
                    {
                        spaText = browser.Single("text", SelectByDataUi).GetInnerText();
                    }
                    //element changed during retrieval of text.
                    catch (StaleElementReferenceException)
                    {
                        continue;
                    }

                    if (spaText != "PAGE 1")
                    {
                        AssertUI.IsDisplayed(progress);
                    }
                    else
                    {
                        return;
                    }

                    Thread.Sleep(50);
                }

                Assert.True(false, "SPA 2 page did not load in time");
            });
        }
        [Fact]
        public void Control_UpdateProgress_SPA_LongAction()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgressRedirectSPA1);
                var progress = browser.Single("progress", By.Id);

                AssertUI.IsNotDisplayed(progress);

                browser.Single("long", SelectByDataUi).Click();
                AssertUI.IsDisplayed(progress);
                Thread.Sleep(1000);
                AssertUI.IsNotDisplayed(progress); //"Update progress did not hide after action finished"
            });
        }

        public UpdateProgressTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
