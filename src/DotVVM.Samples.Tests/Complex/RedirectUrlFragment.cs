using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;
using Riganti.Utils.Testing.SeleniumCore.Exceptions;
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

            base.RunInAllBrowsers(browser =>
            {
                //Postback with no redirect sets message
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_RedirectAndUrl_ScrollingPage);
                browser.First("a[data-ui=test-link]").Click();
                browser.Wait(200);
                browser.First("span[data-ui='message1']").CheckIfInnerText(s => s.Equals("TestMessage"));

                //used RedirectToUrl to redirect to page with Id, however the redirect made page reload and discarted the viewmodel
                //therefore  message1 should be blank
                //view should scroll to #paragraph2
                browser.First("a[data-ui='go-to-2-url-link']").Click();
                browser.Wait(1200);
                // message 2 should be scrolled to message 1 should not, both should be blank
                var message2element = browser.First("span[data-ui='message2']");
                message2element.IsDisplayed();
                message2element.CheckIfIsElementInView();           // TODO: Doesn't work in IE

                var message1element = browser.First("span[data-ui='message1']");
                message1element.IsDisplayed();
                message1element.CheckIfIsElementNotInView();

                message1element.CheckIfInnerText(s => string.IsNullOrEmpty(s));
                message2element.CheckIfInnerText(s => string.IsNullOrEmpty(s));
            });
        }

        [TestMethod]
        public void RedirectUrlFragment_ResultIdFragment_Navigation()
        {
            //There I am testing that scrolling to element using Context.ResultIdFragment works correctly
            //It should scroll to element without interupting the postback

            base.RunInAllBrowsers(browser =>
            {
                //Postback with no redirect sets message to 'TestMessage'
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_RedirectAndUrl_ScrollingPage);
                browser.First("a[data-ui=test-link]").Click();
                browser.Wait(200);
                browser.First("span[data-ui='message1']").CheckIfInnerText(s => s.Equals("TestMessage"));

                //Postback sould run and view should scroll, page should not reload therefore messeges remain.
                browser.First("a[data-ui='go-to-2-link']").Click();
                browser.Wait(200);
                
                var message2element = browser.First("span[data-ui='message2']");
                var message1element = browser.First("span[data-ui='message1']");

                //Message 2 should be scrolled to while message 1 snhould not, and both should have their texts set from the postback. 
                message1element.IsDisplayed();
                message2element.IsDisplayed();

                message1element.CheckIfIsElementNotInView();
                message2element.CheckIfIsElementInView();

                message1element.CheckIfInnerText(s => s.Equals("ToParagraph2"));
                message2element.CheckIfInnerText(s => s.Equals("ToParagraph2"));

                //basicly the same just clicking on link to do postback and scroll back to paragraph1 after
                browser.First("a[data-ui='go-to-1-link']").Click();
                browser.Wait(200);
                // message 2 should be scrolled to message 1 should not, both should be blank

                message2element.IsDisplayed();
                message1element.IsDisplayed();

                message1element.CheckIfIsElementInView();
                message2element.CheckIfIsElementNotInView();

                message1element.CheckIfInnerText(s => s.Equals("ToParagraph1"));
                message2element.CheckIfInnerText(s => s.Equals("ToParagraph1"));

                //Now test that the scrolling works 2 times in row with same link
                var goTo1Link = browser.First("a[data-ui='go-to-1-link']");
                goTo1Link.ScrollTo();
                goTo1Link.CheckIfIsElementInView();
                message1element.CheckIfIsElementNotInView();
                message2element.CheckIfIsElementInView();

                goTo1Link.Click();
                message1element.CheckIfIsElementInView();
                message2element.CheckIfIsElementNotInView();
            });
        }
    }
}

public static class ElementWrapperIsInViewExtensions
{
    public static void ScrollTo(this ElementWrapper element)
    {
        var javascript = @"
            function findPosition(element) {
                var curtop = 0;
                if (element.offsetParent) {
                    do {
                        curtop += element.offsetTop;
                    } while (element = element.offsetParent);
                return [curtop];
                }
            }

            window.scroll(0,findPosition(arguments[0]));
        ";
        var executor = element.BrowserWrapper.GetJavaScriptExecutor();
        executor.ExecuteScript(javascript,element.WebElement);
    }

    public static void CheckIfIsElementInView(this ElementWrapper element)
    {
        if (!IsElementInView(element))
        {
            throw new UnexpectedElementStateException($"Element is not in browser view. {element.ToString()}");
        }
    }

    public static void CheckIfIsElementNotInView(this ElementWrapper element)
    {
        if (IsElementInView(element))
        {
            throw new UnexpectedElementStateException($"Element is in browser view. {element.ToString()}");
        }
    }

    public static bool IsElementInView( this ElementWrapper element)
    {
        var executor = element.BrowserWrapper.GetJavaScriptExecutor();

        var result = executor.ExecuteScript(@"
function elementInViewport2(el) {
  var top = el.offsetTop;
  var left = el.offsetLeft;
  var width = el.offsetWidth;
  var height = el.offsetHeight;

  while(el.offsetParent) {
    el = el.offsetParent;
    top += el.offsetTop;
    left += el.offsetLeft;
  }

  return (
    top < (window.pageYOffset + window.innerHeight) &&
    left < (window.pageXOffset + window.innerWidth) &&
    (top + height) > window.pageYOffset &&
    (left + width) > window.pageXOffset
  );
}

return elementInViewport2(arguments[0]);
                ", element.WebElement);

        return (bool)result;
    }
}
