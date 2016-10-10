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
    public class RadioButtonTests : SeleniumTestBase
    {

        [TestMethod]
        public void Control_RadioButton()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl( SamplesRouteUrls.ControlSamples_RadioButton_RadioButton);
                browser.Wait();

                browser.ElementAt("input[type=radio]", 2).Click();
                browser.ElementAt("input[type=radio]", 3).Click();
                browser.First("input[type=button]").Click();
                browser.Wait();

                browser.Last("span")
                    .CheckIfInnerTextEquals("4");

                browser.ElementAt("input[type=radio]", 1).Click();
                browser.First("input[type=button]").Click();
                browser.Wait();

                browser.Last("span")
                    .CheckIfInnerTextEquals("2");
            });
        }
    }
}