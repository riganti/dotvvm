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
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ServerRendering_ControlUsageSample);
                browser.First("a[data-ui=show-link]").Click();
                browser.Wait(200);
                browser.First("input[data-ui=textbox]").CheckAttribute("value", v => v.StartsWith("a"));
            });
        }

        [TestMethod]
        public void PostbackUpdate_DataContext_RewritingAndHiding()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ServerRendering_ControlUsageSampleRewriting);
                browser.First("a[data-ui=show-link]").Click();
                browser.Wait(200);
                var elem = browser.First("div[data-ui='context-1']").First("input[data-ui=textbox]");
                elem.CheckAttribute("value", v => v.StartsWith("a"));
                browser.First("a[data-ui=rewrite-link]").Click();
                browser.First("div[data-ui='context-2']").First("input[data-ui=textbox]").CheckAttribute("value", v => v.StartsWith("b"), "value does not start with 'b'");
            });
        }
    }
}
