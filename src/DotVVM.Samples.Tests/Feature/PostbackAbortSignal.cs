using System;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.Core.Api;
using Riganti.Selenium.DotVVM;
using Xunit;

namespace DotVVM.Samples.Tests.Feature
{
    public class PostbackAbortSignal : AppSeleniumTest
    {
        public PostbackAbortSignal(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void StaticCommand()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackAbortSignal_LoadAbortViewModel);

                var abortMessageSpan = browser.Single(".message");
                var abortButton = browser.Single("[data-uitest-name=abort]");
                var staticCommandButton = browser.Single("[data-uitest-name=static-command]");

                staticCommandButton.Click();
                abortButton.Click();

                browser.WaitFor(() => {
                    var dataRepeater = browser.Single("[data-uitest-name=data]");
                    AssertUI.InnerTextEquals(abortMessageSpan, "aborted");
                    Assert.False(dataRepeater.Children.Any(), "There are not suposed to be any data");
                }, 4000);

                staticCommandButton.Click();

                browser.Wait(3000);

                browser.WaitFor(() => {
                    var dataRepeater = browser.Single("[data-uitest-name=data]");
                    Assert.Equal(3, dataRepeater.Children.Count());
                }, 4000);
            });
        }

        [Fact]
        public void Command()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostbackAbortSignal_LoadAbortViewModel);

                var abortMessageSpan = browser.Single(".message");
                var abortButton = browser.Single("[data-uitest-name=abort]");
                var staticCommandButton = browser.Single("[data-uitest-name=command]");

                staticCommandButton.Click();
                abortButton.Click();

                browser.WaitFor(() => {
                    var dataRepeater = browser.Single("[data-uitest-name=data]");
                    AssertUI.InnerTextEquals(abortMessageSpan, "aborted");
                    Assert.False(dataRepeater.Children.Any(), "There are not suposed to be any data");
                }, 4000);

                staticCommandButton.Click();

                browser.Wait(3000);

                browser.WaitFor(() => {
                    var dataRepeater = browser.Single("[data-uitest-name=data]");
                    Assert.Equal(3, dataRepeater.Children.Count());
                }, 4000);
            });
        }
    }
}
