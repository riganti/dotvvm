using Riganti.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Xunit.Abstractions;
using Riganti.Selenium.DotVVM;

namespace DotVVM.Samples.Tests.Control
{
    public class RadioButtonTests : AppSeleniumTest
    {
        [Fact]
        public void Control_RadioButton_RadioButton()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RadioButton_RadioButton);
                browser.Wait();

                browser.ElementAt("input[type=radio]", 2).Click();
                browser.ElementAt("input[type=radio]", 3).Click();
                browser.First("input[type=button]").Click();
                browser.Wait();

                AssertUI.InnerTextEquals(browser.Last("span"), "4");

                browser.ElementAt("input[type=radio]", 1).Click();
                browser.First("input[type=button]").Click();
                browser.Wait();

                AssertUI.InnerTextEquals(browser.Last("span"), "2");
            });
        }

        [Fact]
        public void Control_RadioButton_Nullable()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RadioButton_Nullable);
                browser.WaitUntilDotvvmInited();

                var radio1 = browser.Single("radiobutton-first", SelectByDataUi).Single("input");
                var radio2 = browser.Single("radiobutton-second", SelectByDataUi).Single("input");

                // null value
                var span = browser.Single("sample-item", SelectByDataUi);
                AssertUI.InnerTextEquals(span, "");

                radio1.Click();
                browser.WaitFor(() => AssertUI.InnerTextEquals(span, "First"), 1000);

                radio2.Click();
                browser.WaitFor(() => AssertUI.InnerTextEquals(span, "Second"), 1000);

                browser.ElementAt("input[type=button]", 0).Click();
                browser.WaitFor(() => AssertUI.InnerTextEquals(span, "Second"), 1000);
                AssertUI.IsChecked(radio2);

                browser.ElementAt("input[type=button]", 1).Click();
                browser.WaitFor(() => AssertUI.InnerTextEquals(span, ""), 1000);
                AssertUI.IsNotChecked(radio1);
                AssertUI.IsNotChecked(radio2);

                browser.ElementAt("input[type=button]", 2).Click();
                browser.WaitFor(() => AssertUI.InnerTextEquals(span, "First"), 1000);
                AssertUI.IsChecked(radio1);

                browser.ElementAt("input[type=button]", 3).Click();
                browser.WaitFor(() => AssertUI.InnerTextEquals(span, "Second"), 1000);
                AssertUI.IsChecked(radio2);
            });
        }


        [Fact]
        public void Control_RadioButton_RadioButtonObjects()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RadioButton_RadioButton_Objects);

                var radios = browser.FindElements("input[type=radio]");
                var ul = browser.Single("ul");

                AssertUI.IsChecked(radios[0]);
                AssertUI.IsNotChecked(radios[1]);
                AssertUI.IsNotChecked(radios[2]);
                ul.FindElements("li").ThrowIfDifferentCountThan(1);
                AssertUI.TextEquals(ul.ElementAt("li", 0), "1: Red");

                // check second radio
                radios[1].Click();
                ul.FindElements("li").ThrowIfDifferentCountThan(1);
                AssertUI.TextEquals(ul.ElementAt("li", 0), "2: Green");

                // check third check box
                radios[2].Click();
                ul.FindElements("li").ThrowIfDifferentCountThan(1);
                AssertUI.TextEquals(ul.ElementAt("li", 0), "3: Blue");

                // click button
                browser.Single("input[type=button]").Click().Wait(500);
                AssertUI.IsNotChecked(radios[0]);
                AssertUI.IsChecked(radios[1]);
                AssertUI.IsNotChecked(radios[2]);
                ul.FindElements("li").ThrowIfDifferentCountThan(1);
                AssertUI.TextEquals(ul.ElementAt("li", 0), "2: Green");

                AssertUI.TextEquals(radios[2].ParentElement.Single("span"), "Blue");
            });
        }

        public RadioButtonTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
