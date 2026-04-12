using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class LateContentPlaceHoldersTests : AppSeleniumTest
    {
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
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_SharedIdContent))]
        public void Feature_LateContentPlaceHolders_SameContentPlaceHolderIdInRootAndNested()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_SharedIdContent);

                browser.First("h1"); // root master page
                browser.First("h2"); // nested master page (shared ID)
                var pageContent = browser.First("[data-ui='shared-id-page-content']");
                AssertUI.InnerTextEquals(pageContent, "Shared ID Page Content");
                // Default content from the shared ID placeholder should NOT be shown
                AssertUI.IsNotDisplayed(browser.Single("[data-ui='default-shared-content']"));
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_ContentWithDefault))]
        public void Feature_LateContentPlaceHolders_DefaultContentUsedWhenNoContentProvided()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_ContentWithDefault);

                browser.First("h1"); // root master page
                browser.First("h2"); // nested master page
                // Default content from the NestedContent placeholder should be shown
                var defaultContent = browser.First("[data-ui='default-nested-content']");
                AssertUI.InnerTextEquals(defaultContent, "Default nested content");
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

        public LateContentPlaceHoldersTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
