using System.Collections.Generic;
using System.Text;
using DotVVM.Testing.Abstractions;
using Xunit;
using Xunit.Abstractions;
using Assert = Riganti.Utils.Testing.Selenium.Core.Assert;

namespace DotVVM.Samples.Tests.New
{
    public class ButtonTests : AppSeleniumTest
    {
        public ButtonTests(ITestOutputHelper output) : base(output)
        {
        }
        [Fact]
        public void Control_Button_Button()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_Button);
                var resultCheck = browser.First("span.result");
                Assert.InnerText(resultCheck, s => s.Equals("0"), "Text has to be '0'");

                browser.First("input[type=button]").Click();
                Assert.InnerText(resultCheck, s => s.Equals("1"), "Text has to be '1'");
            });
        }
        [Fact]
        public void Control_Button_InputTypeButton_TextContentInside()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_InputTypeButton_TextContentInside);

                Assert.CheckAttribute(browser.First("input[type=button]"),"value", s => s.Equals("This is text"));
            });
        }

        [Fact]
        public void Control_Button_InputTypeButton_HtmlContentInside()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_InputTypeButton_HtmlContentInside);

                Assert.InnerText(browser.First("p.summary"),
                      t =>
                          t.Trim().Contains("DotVVM.Framework.Controls.DotvvmControlException") &&
                          t.Trim().Contains("The <dot:Button> control cannot have inner HTML connect unless the 'ButtonTagName' property is set to 'button'!")
                      , "");
            });
        }

        [Fact]
        public void Control_Button_ButtonTagName()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_ButtonTagName);

                Assert.TagName(browser.First("#ButtonTextProperty"), s => s.Equals("button"));
                Assert.TagName(browser.First("#ButtonTextBinding"), s => s.Equals("button"));
                Assert.TagName(browser.First("#InputTextProperty"), s => s.Equals("input"));
                Assert.TagName(browser.First("#InputTextBinding"), s => s.Equals("input"));
                Assert.TagName(browser.First("#ButtonInnerText"), s => s.Equals("button"));

                Assert.TagName(browser.First("#ButtonTextPropertyUpperCase"), s => s.Equals("button"));
                Assert.TagName(browser.First("#ButtonTextBindingUpperCase"), s => s.Equals("button"));
                Assert.TagName(browser.First("#InputTextPropertyUpperCase"), s => s.Equals("input"));
                Assert.TagName(browser.First("#ButtonInnerTextUpperCase"), s => s.Equals("button"));
            });
        }

        [Fact]
        public void Control_Button_ButtonOnClick()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_ButtonOnclick);

                var onclickResult = browser.First("span.result1");
                var clickResult = browser.First("span.result2");

                Assert.InnerText(clickResult, s => s.Equals(""));
                Assert.InnerText(onclickResult, s => s.Equals(""));

                browser.First("input[type=button]").Click();
                Assert.InnerText(clickResult, s => s.Equals("Changed from command binding"));
                Assert.InnerText(onclickResult, s => s.Contains("Changed from onclick"));
            });
        }

    }
}
