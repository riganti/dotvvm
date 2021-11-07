using System;
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
                var rechartOuterHtml = rechart.GetJsInnerHtml();

                browser.First("command-regenerate").Click();

                rechart = browser.First("rechart-control");
                Assert.NotEqual(rechartOuterHtml, rechart.GetJsInnerHtml());
                rechartOuterHtml = rechart.GetJsInnerHtml();
                browser.First("command-removeDOM").Click();

                rechart = browser.First("rechart-control");
                Assert.True(rechart.GetJsInnerHtml().Trim().Length < "< !--ko if: IncludeInPage-- >< !-- / ko-- >".Length + 10); //+ buffer
                browser.First("command-addDOM").Click();

                rechart = browser.First("rechart-control");
                Assert.NotEqual(rechartOuterHtml, rechart.GetJsInnerHtml());
                rechart.FindElements("line", By.TagName).ThrowIfSequenceEmpty();



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
                template1 = browser.WaitFor(s => s.First("template1", SelectByDataUi));
                browser.FindElements("template2").ThrowIfDifferentCountThan(0);
                AssertUI.IsDisplayed(template1);

                // T1 - NOK, T2 - OK 
                browser.First("template-condition").Click();
                browser.FindElements("template1").ThrowIfDifferentCountThan(0);
                template2 = browser.WaitFor(s => s.First("template2"));
                AssertUI.IsDisplayed(template2);


                browser.WaitFor(s => s.First("template2-command")).Click();
                var result = browser.WaitFor(s => getResult());
                AssertUI.TextEquals(result, "TemplateCommandInvoked");

                browser.WaitFor(s => s.First("template2-clientStaticCommand")).Click();
                result = browser.WaitFor(s => getResult());
                AssertUI.TextEquals(result, "StaticCommandInvoked");

                browser.WaitFor(s => s.First("template2-serverStaticCommand")).Click();
                result = browser.WaitFor(s => getResult());
                AssertUI.TextEquals(result, "ServerStaticCommandInvoked");

            });
        }
    }
}
