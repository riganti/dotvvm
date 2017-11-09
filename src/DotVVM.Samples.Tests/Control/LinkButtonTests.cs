using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class LinkButtonTests : AppSeleniumTest
    {
        [TestMethod]
        public void Control_LinkButton_LinkButton()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_LinkButton_LinkButton);

                browser.First("#ButtonTextProperty").CheckTagName("a");
                browser.First("#ButtonTextBinding").CheckTagName("a");
                browser.First("#ButtonInnerText").CheckTagName("a");

                // try to click on a disabled button
                browser.Click("#EnabledLinkButton");
                browser.Wait();
                browser.Last("span").CheckIfInnerTextEquals("0");

                // enable it
                browser.Click("input[type=checkbox]");
                browser.Wait();
                browser.Click("#EnabledLinkButton");
                browser.Wait();
                browser.Last("span").CheckIfInnerTextEquals("1");

                // try to click on a disabled button again
                browser.Click("#EnabledLinkButton");
                browser.Wait();
                browser.Last("span").CheckIfInnerTextEquals("1");
            });
        }

        [TestMethod]
        public void Control_LinkButton_LinkButtonOnClick()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_LinkButton_LinkButtonOnclick);
                var onclickResult = browser.First("span.result1");
                var clickResult = browser.First("span.result2");
                clickResult.CheckIfInnerText(s => s.Equals(""));
                onclickResult.CheckIfInnerText(s => s.Equals(""));

                browser.Click("#LinkButton");
                clickResult.CheckIfInnerText(s => s.Equals("Changed from command binding"));
                onclickResult.CheckIfInnerText(s => s.Contains("Changed from onclick"));
            });
        }
    }
}
