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

        public RadioButtonTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
