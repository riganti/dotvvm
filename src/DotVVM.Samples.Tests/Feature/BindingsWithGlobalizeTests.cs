using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class BindingsWithGlobalizeTests : AppSeleniumTest
    {
        /// <summary>
        /// This tests whether zero value is rendered or not.
        /// </summary>
        [Fact]
        public void Feature_BindingsWithGlobalizeTests_ZeroValue()
        {
            // When dotvvm_globalize was edited there was a bug that caused that zero was not rendered. The zero value was resolved as a false value and formatting of the value was stopped.
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_LiteralBinding_LiteralBinding_Zero);
                browser.WaitUntilDotvvmInited();
                AssertUI.TextEquals(browser.FirstOrDefault("#zeroSpan"), 0.ToString()); ;
            });
        }

        public BindingsWithGlobalizeTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
