using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class ButtonTests : SeleniumTest
    {

        [TestMethod]
        public void Control_Button_Button()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_Button);

                browser.First("span.result").CheckIfInnerTextEquals("0");
                browser.First("input[type=button]").Click();
                browser.First("span.result").CheckIfInnerTextEquals("1");
            });
        }

        [TestMethod]
        public void Control_Button_InputTypeButton_TextContentInside()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_InputTypeButton_TextContentInside);

                browser.First("input[type=button]")
                    .CheckAttribute("value", "This is text");
            });
        }

        [TestMethod]
        public void Control_Button_InputTypeButton_HtmlContentInside()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_InputTypeButton_HtmlContentInside);

                browser.First("p.summary")
                    .CheckIfInnerText(t => t.Contains("DotVVM.Framework.Controls.DotvvmControlException")
                        && t.Contains("The <dot:Button> control cannot have inner HTML connect unless the 'ButtonTagName' property is set to 'button'!"));
            });
        }

        [TestMethod]
        public void Control_Button_ButtonTagName()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_ButtonTagName);

                browser.First("#ButtonTextProperty").CheckTagName("button");
                browser.First("#ButtonTextBinding").CheckTagName("button");
                browser.First("#InputTextProperty").CheckTagName("input");
                browser.First("#InputTextBinding").CheckTagName("input");
                browser.First("#ButtonInnerText").CheckTagName("button");

                browser.First("#ButtonTextPropertyUpperCase").CheckTagName("button");
                browser.First("#ButtonTextBindingUpperCase").CheckTagName("button");
                browser.First("#InputTextPropertyUpperCase").CheckTagName("input");
                browser.First("#ButtonInnerTextUpperCase").CheckTagName("button");
            });
        }

    }
}