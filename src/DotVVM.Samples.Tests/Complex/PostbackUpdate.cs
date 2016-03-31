using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;
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
                browser.Wait(200);
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
                browser.Wait(200);
                browser.First("div[data-ui='context-1']").First("input[data-ui=textbox]").CheckAttribute("value", v => v.Contains("a"));
                browser.First("a[data-ui=rewrite-link]").Click();
                browser.First("div[data-ui='context-2']").First("input[data-ui=textbox]").CheckAttribute("value", v => v.Contains("b"));
            });
        }
    }
}
