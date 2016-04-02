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
            //Expected: Page reloads and scroolls to element-id

            RunInAllBrowsers(browser =>
            {
                //Postback with no redirect sets message
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_RedirectAndUrl_ScrollingPage);
                browser.First("a[data-ui=test-link]").Click();
                browser.Wait(200);
                browser.First("span[data-ui='message1']").CheckIfInnerText(s=> s.Equals("TestMessage"));

                //used RedirectToUrl to redirect to page with Id, however the redirect made page reload and discarted the viewmodel
                //therefore  message1 should be blank
                browser.First("a[data-ui='go-to-2-url-link']").Click();
                browser.Wait(200);
                browser.CheckIfIsDisplayed("span[data-ui='message1']");
                browser.First("span[data-ui='message2']").CheckIfInnerText(s => string.IsNullOrEmpty(s));
            });
        }
    }
}
