using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;

namespace DotVVM.Samples.Tests.Feature
{
    public class GenericTypesTests : AppSeleniumTest
    {
        public GenericTypesTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_GenericTypes_InResourceBinding()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_GenericTypes_InResourceBinding);

                AssertUI.InnerTextEquals(browser
                    .Single("span[data-ui=generic-instance-function]"),
                    "Hello from instance generic method arg1:Hallo from generic parameter. arg2:Hallo from generic parameter.");

                AssertUI.InnerTextEquals(browser
                    .Single("span[data-ui=generic-class-full]"),
                    "Hallo from generic parameter.");

                AssertUI.InnerTextEquals(browser
                    .Single("span[data-ui=generic-class-aliased]"),
                    "Hallo from generic parameter.");

                AssertUI.InnerTextEquals(browser
                    .Single("span[data-ui=generic-static-function-aliased]"),
                    "Hello from static generic method arg1:Hallo from generic parameter. arg2:Hallo from generic parameter.");
            });
        }

        [Theory]
        [InlineData(SamplesRouteUrls.FeatureSamples_GenericTypes_InCommandBinding)]
        [InlineData(SamplesRouteUrls.FeatureSamples_GenericTypes_InStaticCommandBinding)]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_GenericTypes_InStaticCommandBinding))]
        public void Feature_GenericTypes_InCommandBinding(string url)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(url);

                browser.Single("input[data-ui=static-command]").Click();
                browser.Single("input[data-ui=instance-command]").Click();

                AssertUI.InnerTextEquals(browser
                    .Single("span[data-ui=instance-output]"),
                    "Hello from instance generic command arg1:Hallo from generic parameter. arg2:Hallo from generic parameter.");

                AssertUI.InnerTextEquals(browser
                    .Single("span[data-ui=static-output]"),
                    "Hello from static generic command arg1:Hallo from generic parameter. arg2:Hallo from generic parameter.");
            });
        }
    }
}
