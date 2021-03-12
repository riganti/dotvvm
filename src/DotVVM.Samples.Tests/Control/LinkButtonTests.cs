using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class LinkButtonTests : AppSeleniumTest
    {
        [Fact]
        public void Control_LinkButton_LinkButton()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_LinkButton_LinkButton);

                AssertUI.TagName(browser.First("#ButtonTextProperty"), "a");
                AssertUI.TagName(browser.First("#ButtonTextBinding"), "a");
                AssertUI.TagName(browser.First("#ButtonInnerText"), "a");

                // try to click on a disabled button
                browser.Click("#EnabledLinkButton");
                
                AssertUI.InnerTextEquals(browser.Last("span"), "0");

                // enable it
                browser.Click("input[type=checkbox]");
                
                browser.Click("#EnabledLinkButton");
                
                AssertUI.InnerTextEquals(browser.Last("span"), "1");

                // try to click on a disabled button again
                browser.Click("#EnabledLinkButton");
                
                AssertUI.InnerTextEquals(browser.Last("span"), "1");
            });
        }

        [Fact]
        public void Control_LinkButton_LinkButtonOnclick()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_LinkButton_LinkButtonOnclick);
                var onclickResult = browser.First("span.result1");
                var clickResult = browser.First("span.result2");
                AssertUI.InnerText(clickResult, s => s.Equals(""));
                AssertUI.InnerText(onclickResult, s => s.Equals(""));

                browser.Click("#LinkButton");
                AssertUI.InnerText(clickResult, s => s.Equals("Changed from command binding"));
                AssertUI.InnerText(onclickResult, s => s.Contains("Changed from onclick"));
            });
        }

        [Fact]
        public void Control_LinkButton_LinkButtonEnabled()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_LinkButton_LinkButtonEnabled);

                var commandResult = browser.First("[data-ui=command-result]");
                var staticCommandResult = browser.First("[data-ui=static-command-result]");
                var clientStaticCommandResult = browser.First("[data-ui=client-static-command-result]");

                AssertUI.InnerTextEquals(commandResult, "");
                AssertUI.InnerTextEquals(staticCommandResult, "");
                AssertUI.InnerTextEquals(clientStaticCommandResult, "");

                var commandLinkButton = browser.First("[data-ui=command-linkbutton]");
                var staticCommandLinkButton = browser.First("[data-ui=static-command-linkbutton]");
                var clientStaticCommandLinkButton = browser.First("[data-ui=client-static-command-linkbutton]");

                commandLinkButton.Click();
                staticCommandLinkButton.Click();
                clientStaticCommandLinkButton.Click();

                AssertUI.InnerTextEquals(commandResult, "");
                AssertUI.InnerTextEquals(staticCommandResult, "");
                AssertUI.InnerTextEquals(clientStaticCommandResult, "");

                browser.First("[data-ui=toggle-enabled]").Click();

                commandLinkButton.Click();
                staticCommandLinkButton.Click();
                clientStaticCommandLinkButton.Click();

                AssertUI.InnerTextEquals(commandResult, "Changed from command binding");
                AssertUI.InnerTextEquals(staticCommandResult, "Changed from static command on server");
                AssertUI.InnerTextEquals(clientStaticCommandResult, "Changed from static command");
            });
        }

        public LinkButtonTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
