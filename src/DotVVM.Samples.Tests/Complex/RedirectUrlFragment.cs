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
    public class RedirectUrlFragment : SeleniumTestBase
    {
        [TestMethod]
        public void RedirectUrlFragment_PostbackInteruption()
        {
            //When redirecting to fragment e.g. /uri#element-id postback gets interupted and the page does not reload 
            //so As I am writing this sample scrolls OK but messages ToParagraph2, ToParagraph1 don't show.
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_RedirectAndUrl_ScrollingPage);
                browser.First("a[data-ui=test-link]").Click();
                browser.Wait(200);
                browser.First("span[data-ui='message1']").CheckIfInnerText(s=> s.Equals("TestMessage"));

                browser.First("a[data-ui='go-to-2-link']").Click();
                browser.Wait(200);
                browser.CheckIfIsDisplayed("span[data-ui='message2']");
                browser.First("span[data-ui='message2']").CheckIfInnerText(s => s.Equals("ToParagraph2"));

                browser.First("a[data-ui='go-to-1-link']").Click();
                browser.Wait(200);
                browser.IsDisplayed("span[data-ui='message1']");
                browser.First("span[data-ui='message1']").CheckIfInnerText(s => s.Equals("ToParagraph1"));
            });
        }
    }
}
