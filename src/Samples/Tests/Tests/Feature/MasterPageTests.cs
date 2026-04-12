using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
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

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_Content))]
        public void Feature_LateContentPlaceHolders_ContentPlaceHolderInTemplate()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_Content);

                browser.First("h1"); // root master page
                browser.First("h2"); // nested master page
                var pageContent = browser.First("[data-ui='page-content']");
                AssertUI.InnerTextEquals(pageContent, "Page Content - Late ContentPlaceHolder works!");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_MismatchedContent))]
        [Trait("Category", "dev-only")] // tests the error page behavior
        public void Feature_LateContentPlaceHolders_MismatchedContent_ThrowsError()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_MismatchedContent);
                // The page should show an error because Content(NonExistentPlaceHolder) has no matching ContentPlaceHolder
                AssertUI.InnerText(browser.First(".exceptionMessage"), s => s.Contains("NonExistentPlaceHolder"));
            });
        }

        public MasterPageTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
