using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class CheckBoxInRepeaterTests:SeleniumTestBase
    {
        [TestMethod]
        public void CheckBoxInRepeaterTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_CheckboxInRepeater);

                browser.Single("#checkbox-one").Click();
                browser.Single("#checkbox-one").CheckIfIsChecked();
                browser.First("span").CheckIfInnerText(s => s.Contains("one"));

                browser.Single("#checkbox-two").Click();
                browser.Single("#checkbox-two").CheckIfIsChecked();
                browser.First("span").CheckIfInnerText(s => s.Contains("one") && s.Contains("two"));

                browser.Single("#checkbox-three").Click();
                browser.Single("#checkbox-three").CheckIfIsChecked();
                browser.First("span").CheckIfInnerText(s => s.Contains("one") && s.Contains("two") && s.Contains("three"));

                browser.First("#set-server-values").Click();
                browser.Single("#checkbox-one").CheckIfIsChecked();
                browser.Single("#checkbox-three").CheckIfIsChecked();
                browser.First("span").CheckIfInnerText(s => s.Contains("one") && s.Contains("three"));
            });
        }
    }
}
