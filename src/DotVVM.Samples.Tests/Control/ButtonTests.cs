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
                var resultCheck = browser.First("span.result").Check();
                resultCheck.InnerText(s => s.Equals("0"));

                browser.First("input[type=button]").Click();
                resultCheck.InnerText(s => s.Equals("1"));
            });
        }

        [TestMethod]
        public void Control_Button_InputTypeButton_TextContentInside()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_InputTypeButton_TextContentInside);

                browser.First("input[type=button]")
                    .Check().Attribute("value", s => s.Equals("This is text"));
            });
        }

        [TestMethod]
        public void Control_Button_InputTypeButton_HtmlContentInside()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_InputTypeButton_HtmlContentInside);

                browser.First("p.summary")
                    .Check().InnerText(t =>
                    {
                        t.Trim = true;
                        t.Contains("DotVVM.Framework.Controls.DotvvmControlException");
                        t.Contains("The <dot:Button> control cannot have inner HTML connect unless the 'ButtonTagName' property is set to 'button'!");
                    }, "");
            });
        }

        [TestMethod]
        public void Control_Button_ButtonTagName()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_ButtonTagName);

                browser.First("#ButtonTextProperty").Check().Tag(s => s.Equals("button"));
                browser.First("#ButtonTextBinding").Check().Tag(s => s.Equals("button"));
                browser.First("#InputTextProperty").Check().Tag(s => s.Equals("input"));
                browser.First("#InputTextBinding").Check().Tag(s => s.Equals("input"));
                browser.First("#ButtonInnerText").Check().Tag(s => s.Equals("button"));

                browser.First("#ButtonTextPropertyUpperCase").Check().Tag(s => s.Equals("button"));
                browser.First("#ButtonTextBindingUpperCase").Check().Tag(s => s.Equals("button"));
                browser.First("#InputTextPropertyUpperCase").Check().Tag(s => s.Equals("input"));
                browser.First("#ButtonInnerTextUpperCase").Check().Tag(s => s.Equals("button"));
            });
        }

    }
}