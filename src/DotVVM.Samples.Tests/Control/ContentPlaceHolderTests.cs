using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class ContentPlaceHolderTests : SeleniumTestBase
    {
        [TestMethod]
        public void EmptyContentPlaceHolderTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ContentPlaceHolder_ContentPlaceHolderPage);
                browser.First("#innerHtmlTest").CheckIfJsPropertyInnerHtml(html => string.IsNullOrWhiteSpace(System.Net.WebUtility.HtmlDecode(html)));
            });
        }
    }
}
