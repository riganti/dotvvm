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
                browser.FindElements("[data-ui='default-shared-content']").ThrowIfDifferentCountThan(0);
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

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_RepeaterOneItem))]
        public void Feature_LateContentPlaceHolders_ContentPlaceHolderInRepeater_OneItem_Works()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_RepeaterOneItem);

                browser.First("[data-ui='master-heading']"); // master page rendered
                var items = browser.FindElements("[data-ui='repeater-item']");
                Assert.Equal(1, items.Count);
                AssertUI.InnerTextEquals(items[0], "Item 1");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_RepeaterZeroItems))]
        [Trait("Category", "dev-only")] // tests the error page behavior
        public void Feature_LateContentPlaceHolders_ContentPlaceHolderInRepeater_ZeroItems_ThrowsError()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_RepeaterZeroItems);
                // Should throw because ContentPlaceHolder is never instantiated (Repeater has no items)
                AssertUI.InnerText(browser.First(".exceptionMessage"), s => s.Contains("RepeaterContent"));
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_RepeaterMultipleItems))]
        [Trait("Category", "dev-only")] // tests the error page behavior
        public void Feature_LateContentPlaceHolders_ContentPlaceHolderInRepeater_MultipleItems_ThrowsError()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_RepeaterMultipleItems);
                // Should throw because ContentPlaceHolder is instantiated more than once (Repeater has 2+ items)
                AssertUI.InnerText(browser.First(".exceptionMessage"), s => s.Contains("RepeaterContent") && s.Contains("already been resolved"));
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_AuthViewContent))]
        public void Feature_LateContentPlaceHolders_SameContentPlaceHolderIdInAuthenticatedViewTemplates_Works()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_LateContentPlaceHolders_AuthViewContent);

                browser.First("[data-ui='master-heading']"); // master page rendered
                // In unauthenticated state, the NotAuthenticatedTemplate is instantiated
                var section = browser.First("[data-ui='not-authenticated-section']");
                var content = section.First("[data-ui='auth-content']");
                AssertUI.InnerText(content, s => s.Contains("Content from ContentPlaceHolder inside AuthenticatedView template"));
            });
        }

        public LateContentPlaceHoldersTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
