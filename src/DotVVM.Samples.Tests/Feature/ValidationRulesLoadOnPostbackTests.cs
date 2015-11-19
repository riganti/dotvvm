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
        [TestMethod]
        public void Feature_ValidationRulesLoadOnPostback()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ValidationRulesLoadOnPostback);

                // click the validate button
                browser.FindElements("input[type=button]").Last().Click();
                browser.Wait();

                // ensure validators are hidden
                Assert.AreEqual("true", browser.FindElements("span").Last().GetText());
                Assert.AreEqual(0, browser.FindElements("li").Count());

                // load the customer
                browser.Click("input[type=button]");
                browser.Wait();

                // try to validate
                browser.FindElements("input[type=button]").Last().Click();
                browser.Wait();
                //(Assert.AreEqual(1, browser.FindElements("li").Count());
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                //Assert.IsTrue(browser.First("li").GetText().Contains("Email"));
                browser.First("li").CheckIfInnerText(s => s.Contains("Email"));

                // fix the e-mail address
                browser.FindElements("input[type=text]").Last().Clear();
                browser.FindElements("input[type=text]").Last().SendKeys("test@mail.com");
                browser.FindElements("input[type=button]").Last().Click();
                browser.Wait();

                // try to validate
                browser.FindElements("span").Last().CheckIfInnerTextEquals("true");
                browser.FindElements("li").ThrowIfDifferentCountThan(0);
            });
        }
    }
}
