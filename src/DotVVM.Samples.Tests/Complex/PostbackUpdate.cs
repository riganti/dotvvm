using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;
using Riganti.Utils.Testing.Selenium.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class PostbackUpdate : SeleniumTestBase
    {
        [TestMethod]
        public void PostbackUpdate_DataContext_WrongUpdateOnPostback()
        {
            //As I am writing this, test should fail because on postback there will be two <!-- ko with: EditedArticle --/> elements inside each other instead of one.
            //Caused by DataContext, Visible, RenderSettings.Mode, PostBack.Update being all on the same div
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ServerRendering_ControlUsageSample);
                browser.First("a[data-ui=show-link]").Click();
                browser.Wait(500);
                browser.First("input[data-ui=textbox]").CheckAttribute("value", v => v.Contains("a"));
            });
        }

        [TestMethod]
        public void PostbackUpdate_DataContext_RewritingAndHiding()
        {
            //As I am writing this, test should work because RenderSettings.Mode, PostBack.Update are on div that is inside div with DataContext.
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ServerRendering_ControlUsageSampleRewriting);
                browser.First("a[data-ui=show-link]").Click();
                browser.Wait(500);
                browser.First("div[data-ui='context-1']").First("input[data-ui=textbox]").CheckAttribute("value", v => v.Contains("a"));
                browser.First("a[data-ui=rewrite-link]").Click();
                browser.Wait(500);
                browser.First("div[data-ui='context-2']").First("input[data-ui=textbox]").CheckAttribute("value", v => v.Contains("b"));
            });
        }

        [TestMethod]
        public void PostbackUpdate_ServerRendering_AddingIntoRepeater()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ServerRendering_AddingIntoEmptyRepeater);

                //Is the item on nonempty repeater displayed?
                var articles = browser.Single("div[data-ui='nonempty-repeater']").FindElements("article[data-ui='test-article']");
                CheckArticleCount(browser, "nonempty-repeater", 1);
                articles.SingleOrDefault().Single("span[data-ui='detail-text']").CheckIfInnerTextEquals("NonEmptyArticles 1");

                //Add element and see
                browser.First("a[data-ui='add-link']").Click();
                browser.Wait(500);

                //Has nonempty repeater been updated? 
                var neArticlesPostAdd = browser.Single("div[data-ui='nonempty-repeater']").FindElements("article[data-ui='test-article']");
                CheckArticleCount(browser, "nonempty-repeater", 2);
                neArticlesPostAdd[1].Single("span[data-ui='detail-text']").CheckIfInnerTextEquals("NonEmptyArticles 2");

                //Has the empty one?
                var eArticlesPostAdd = browser.Single("div[data-ui='empty-repeater']").FindElements("article[data-ui='test-article']");
                CheckArticleCount(browser, "empty-repeater", 1);
                eArticlesPostAdd.SingleOrDefault().Single("span[data-ui='detail-text']").CheckIfInnerTextEquals("EmptyArticles 1");
            });
        }

        public static void  CheckArticleCount(BrowserWrapper browser, string repeaterUiId, int expectedCount)
        {
            var articles = browser.First($"div[data-ui='{repeaterUiId}']").FindElements("article[data-ui='test-article']");
            if (articles.Count != expectedCount)
            {
                throw new UnexpectedElementStateException($"There should be only 2 article in the repeater. There are {articles.Count}");
            }
        }
    }
}
