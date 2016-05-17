using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ValidationRulesLoadOnPostbackTests : SeleniumTestBase
    {
        /// <summary>
        /// Feature_s the validation rules load on postback.
        /// </summary>
        [TestMethod]
        [Timeout(120000)]
        public void Feature_ValidationRulesLoadOnPostback()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ValidationRulesLoadOnPostback);

                // click the validate button
                browser.Last("input[type=button]").Click();
                browser.Wait();

                // ensure validators are hidden
                browser.Last("span").CheckIfInnerTextEquals("true");
                browser.FindElements("li").ThrowIfDifferentCountThan(0);
                // load the customer
                browser.Click("input[type=button]");

                // try to validate
                browser.Last("input[type=button]").Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                browser.First("li").CheckIfInnerText(s => s.Contains("Email"));

                // fix the e-mail address
                browser.Last("input[type=text]").Clear();
                browser.Last("input[type=text]").SendKeys("test@mail.com");
                browser.Last("input[type=button]").Click();

                // try to validate
                browser.Last("span").CheckIfInnerTextEquals("true");
                browser.FindElements("li").ThrowIfDifferentCountThan(0);
            });
        }
    }
}
