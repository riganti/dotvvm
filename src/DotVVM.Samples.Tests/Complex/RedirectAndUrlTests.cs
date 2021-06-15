using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.Core.Abstractions.Exceptions;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    public class RedirectAndUrlTests : AppSeleniumTest
    {
        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ComplexSamples_RedirectAndUrl_ScrollingPage))]
        public void Complex_RedirectAndUrl_PostbackInteruption()
        {
            //When redirecting to fragment e.g. /uri#element-id postback gets interrupted and the page does not reload
            //Expected: Page reloads and scrolls to element-id

            RunInAllBrowsers(browser => {
                //Postback with no redirect sets message
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_RedirectAndUrl_ScrollingPage);
                browser.First("a[data-ui=test-link]").Click();
                AssertUI.InnerTextEquals(browser.First("span[data-ui='message1']"), "TestMessage");

                //used RedirectToUrl to redirect to page with Id, however the redirect made page reload and discarted the viewmodel
                //therefore  message1 should be blank
                //view should scroll to #paragraph2
                browser.First("a[data-ui='go-to-2-url-link']").Click();
                // message 2 should be scrolled to message 1 should not, both should be blank
                var message2element = browser.First("span[data-ui='message2']");
                message2element.IsDisplayed();
                message2element.CheckIfIsElementInView();           // TODO: Doesn't work in IE

                var message1element = browser.First("span[data-ui='message1']");
                message1element.IsDisplayed();
                message1element.CheckIfIsElementNotInView();

                AssertUI.InnerTextEquals(message1element, "TestMessage");
                AssertUI.InnerTextEquals(message2element, "TestMessage");
            });
        }

        [Fact]
        public void Complex_RedirectAndUrl_ScrollingPage()
        {
            //There I am testing that scrolling to element using Context.ResultIdFragment works correctly
            //It should scroll to element without interrupting the postback

            RunInAllBrowsers(browser => {
                //Postback with no redirect sets message to 'TestMessage'
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_RedirectAndUrl_ScrollingPage);
                browser.First("a[data-ui=test-link]").Click();
                AssertUI.InnerText(browser.First("span[data-ui='message1']"), s => s.Equals("TestMessage"));

                //Postback should run and view should scroll, page should not reload therefore messeges remain.
                browser.First("a[data-ui='go-to-2-link']").Click();

                var message2element = browser.First("span[data-ui='message2']");
                var message1element = browser.First("span[data-ui='message1']");

                //Message 2 should be scrolled to while message 1 should not, and both should have their texts set from the postback.
                message1element.IsDisplayed();
                message2element.IsDisplayed();

                message1element.CheckIfIsElementNotInView();
                message2element.CheckIfIsElementInView();

                AssertUI.InnerText(message1element, s => s.Equals("ToParagraph2"));
                AssertUI.InnerText(message2element, s => s.Equals("ToParagraph2"));

                //basically the same just clicking on link to do postback and scroll back to paragraph1 after
                browser.First("a[data-ui='go-to-1-link']").Click();
                // message 2 should be scrolled to message 1 should not, both should be blank

                message2element.IsDisplayed();
                message1element.IsDisplayed();

                message1element.CheckIfIsElementInView();
                message2element.CheckIfIsElementNotInView();

                AssertUI.InnerText(message1element, s => s.Equals("ToParagraph1"));
                AssertUI.InnerText(message2element, s => s.Equals("ToParagraph1"));

                //Now test that the scrolling works 2 times in row with same link
                var goTo1Link = browser.First("a[data-ui='go-to-1-link']");
                goTo1Link.ScrollTo();
                goTo1Link.CheckIfIsElementInView();
                message1element.CheckIfIsElementNotInView();
                message2element.CheckIfIsElementInView();

                goTo1Link.Click();
                browser.WaitForPostback();
                message1element.CheckIfIsElementInView();
                message2element.CheckIfIsElementNotInView();
            });
        }

        public RedirectAndUrlTests(ITestOutputHelper output) : base(output)
        {
        }
    }

    public static class ElementWrapperIsInViewExtensions
    {
        public static IElementWrapper ScrollTo(this IElementWrapper element)
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
            executor.ExecuteScript(javascript, element.WebElement);
            return element;
        }

        public static void CheckIfIsElementInView(this IElementWrapper element)
        {
            if (!IsElementInView(element))
            {
                throw new UnexpectedElementStateException($"Element is not in browser view. {element.ToString()}");
            }
        }

        public static void CheckIfIsElementNotInView(this IElementWrapper element)
        {
            if (IsElementInView(element))
            {
                throw new UnexpectedElementStateException($"Element is in browser view. {element.ToString()}");
            }
        }

        public static bool IsElementInView(this IElementWrapper element)
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
}
