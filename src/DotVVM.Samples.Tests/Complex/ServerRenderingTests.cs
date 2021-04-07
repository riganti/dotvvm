using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.Core.Abstractions.Exceptions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    public class ServerRenderingTests : AppSeleniumTest
    {
        [Fact]
        public void Complex_ServerRendering_ControlUsageSample()
        {
            //As I am writing this, test should fail because on postback there will be two <!-- ko with: EditedArticle --/> elements inside each other instead of one.
            //Caused by DataContext, Visible, RenderSettings.Mode, PostBack.Update being all on the same div
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ServerRendering_ControlUsageSample);
                browser.First("a[data-ui=show-link]").Click();
                AssertUI.Attribute(browser.First("input[data-ui=textbox]"), "value", v => v.Contains("a"));
            });
        }

        [Fact]
        public void Complex_ServerRendering_ControlUsageSampleRewriting()
        {
            //As I am writing this, test should work because RenderSettings.Mode, PostBack.Update are on div that is inside div with DataContext.
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ServerRendering_ControlUsageSampleRewriting);
                browser.First("a[data-ui=show-link]").Click();
                AssertUI.Attribute(browser.First("div[data-ui='context-1']").First("input[data-ui=textbox]"), "value", v => v.Contains("a"));
                browser.First("a[data-ui=rewrite-link]").Click();
                AssertUI.Attribute(browser.First("div[data-ui='context-2']").First("input[data-ui=textbox]"), "value", v => v.Contains("b"));
            });
        }

        [Fact]
        public void Complex_ServerRendering_AddingIntoEmptyRepeater()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ServerRendering_AddingIntoEmptyRepeater);

                //Is the item on nonempty repeater displayed?
                var articles = browser.Single("div[data-ui='nonempty-repeater']").FindElements("article[data-ui='test-article']");
                CheckArticleCount(browser, "nonempty-repeater", 1);
                AssertUI.InnerTextEquals(articles.SingleOrDefault().Single("span[data-ui='detail-text']"), "NonEmptyArticles 1");

                //Add element and see
                browser.First("a[data-ui='add-link']").Click();

                //Has nonempty repeater been updated?
                var neArticlesPostAdd = browser.Single("div[data-ui='nonempty-repeater']").FindElements("article[data-ui='test-article']");
                CheckArticleCount(browser, "nonempty-repeater", 2);
                AssertUI.InnerTextEquals(neArticlesPostAdd[1].Single("span[data-ui='detail-text']"), "NonEmptyArticles 2");

                //Has the empty one?
                var eArticlesPostAdd = browser.Single("div[data-ui='empty-repeater']").FindElements("article[data-ui='test-article']");
                CheckArticleCount(browser, "empty-repeater", 1);
                AssertUI.InnerTextEquals(eArticlesPostAdd.SingleOrDefault().Single("span[data-ui='detail-text']"), "EmptyArticles 1");
            });
        }

        [Fact]
        public void Complex_ServerRendering_MarkupControlInRepeaterEditing()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ServerRendering_MarkupControlInRepeaterEditing);

                // Does the repeater display the correct number of articles?
                var repeater = browser.Single("div[data-ui='repeater']");
                CheckArticleCount(browser, "repeater", 4);

                // Get message from an article
                var articleDetail = repeater.ElementAt("div", 0);
                var message = articleDetail.Single("span[data-ui='detail-text']").GetText();

                // Click 'Edit'
                articleDetail.Single("a").Click();

                // Check if the textbox contains the same message
                repeater = browser.Single("div[data-ui='repeater']");
                var articleEditor = repeater.ElementAt("div", 1);
                AssertUI.TextEquals(articleEditor.Single("input[data-ui='textbox']"), message);
            });
        }

        public static void CheckArticleCount(IBrowserWrapper browser, string repeaterUiId, int expectedCount)
        {
            var articles = browser.First($"div[data-ui='{repeaterUiId}']").FindElements("article[data-ui='test-article']");
            if (articles.Count != expectedCount)
            {
                throw new UnexpectedElementStateException($"There should be only 2 article in the repeater. There are {articles.Count}");
            }
        }

        public ServerRenderingTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
