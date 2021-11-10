using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class JsComponentIntegrationTests : AppSeleniumTest
    {
        public JsComponentIntegrationTests(ITestOutputHelper output) : base(output)
        {

        }

        [Fact]
        public void Feature_JsComponentIntegrationTests_ReactComponentIntegration_Recharts()
        {
            RunInAllBrowsers(browser => {
                browser.SelectMethod = SelectByDataUi;
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JsComponentIntegration_ReactComponentIntegration);
                browser.WaitUntilDotvvmInited();

                var rechart = browser.First("rechart-control");
                List<string> pathsList = null;
                WaitForExecutor.WaitFor(() => {
                    var paths = browser.FindElements(".recharts-line > path", By.CssSelector);
                    paths.ThrowIfSequenceEmpty();
                    pathsList = paths.Select(s => s.GetAttribute("d")).Where(s => !string.IsNullOrWhiteSpace(s)).OrderBy(s=> s).ToList();
                });

                browser.First("command-removeDOM").Click();
                browser.FindElements(".recharts-line > path", By.CssSelector).ThrowIfDifferentCountThan(0);

                browser.First("command-addDOM").Click();
                browser.FindElements(".recharts-line > path", By.CssSelector).ThrowIfSequenceEmpty();

                List<string> pathsList2 = null;
                WaitForExecutor.WaitFor(() => {
                    var paths = browser.FindElements("path", By.CssSelector);
                    paths.ThrowIfSequenceEmpty(WaitForOptions.Disabled);
                });

                browser.First("command-regenerate").Click();

                WaitForExecutor.WaitFor(() => {
                    var paths = browser.FindElements(".recharts-line > path", By.CssSelector);
                    paths.ThrowIfSequenceEmpty(WaitForOptions.Disabled);
                    var pathsList3 = paths.Select(s => s.GetAttribute("d")).Where(s=> !string.IsNullOrWhiteSpace(s)).OrderBy(s => s).ToList();
                    Assert.NotEqual(pathsList, pathsList2);
                });

            });
        }
        [Fact]
        public void Feature_JsComponentIntegrationTests_ReactComponentIntegration_TemplateSelector()
        {
            RunInAllBrowsers(browser => {
                browser.SelectMethod = SelectByDataUi;
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JsComponentIntegration_ReactComponentIntegration);
                browser.WaitUntilDotvvmInited();

                var getResult = new Func<IElementWrapper>(() => browser.First("result"));

                // T1 - OK, T2 - NOK 
                var template1 = browser.WaitFor(s => s.First("template1", SelectByDataUi));
                browser.FindElements("template2").ThrowIfDifferentCountThan(0);
                AssertUI.IsDisplayed(template1);


                // T1 - NOK, T2 - OK 
                browser.First("template-condition").Click();
                browser.FindElements("template1").ThrowIfDifferentCountThan(0);
                var template2 = browser.WaitFor(s => s.First("template2", SelectByDataUi));
                AssertUI.IsDisplayed(template2);

                // T1 - OK, T2 - NOK 
                browser.First("template-condition").Click();
                browser.WaitFor(() => {
                    template1 = browser.First("template1", SelectByDataUi);
                }, 8000);
                browser.FindElements("template2").ThrowIfDifferentCountThan(0);
                AssertUI.IsDisplayed(template1);

                // T1 - NOK, T2 - OK 
                browser.First("template-condition").Click();
                browser.FindElements("template1").ThrowIfDifferentCountThan(0);
                template2 = browser.WaitFor(s => s.First("template2"));
                AssertUI.IsDisplayed(template2);


                browser.WaitFor(s => s.First("template2-command")).Click();
                IElementWrapper result = null;
                browser.WaitFor(() => result = getResult(), 8000);
                AssertUI.TextEquals(result, "CommandInvoked");

                browser.WaitFor(s => s.First("template2-clientStaticCommand")).Click();
                result = null;
                browser.WaitFor(() => result = getResult(), 8000);
                AssertUI.TextEquals(result, "StaticCommandInvoked");

                browser.WaitFor(s => s.First("template2-serverStaticCommand")).Click();
                result = null;
                browser.WaitFor(() => result = getResult(), 8000);
                AssertUI.TextEquals(result, "ServerStaticCommandInvoked");

            });
        }
    }
}
