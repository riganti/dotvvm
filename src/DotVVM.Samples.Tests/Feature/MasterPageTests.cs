using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class MasterPageTests : AppSeleniumTest
    {
        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_NestedMasterPages_Content))]
        public void Feature_NestedMasterPages_Content_TwoNestedMasterPages()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_NestedMasterPages_Content);
                browser.First("h1"); // root masterpage
                browser.First("h2"); // nested masterpage
                browser.First("h3"); // nested page
            });
        }

        public MasterPageTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
