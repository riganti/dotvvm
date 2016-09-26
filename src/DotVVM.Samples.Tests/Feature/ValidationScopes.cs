using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ValidationScopes: SeleniumTestBase
    {
        [TestMethod]
        public void Feature_ValidationScopesTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ValidationScopes);

                browser.First("input[type=button]").Click();

                browser.First("li").CheckIfInnerText(i => i.Contains("The Value field is required."));
            });
        }

        [TestMethod]
        public void Feature_ValidationScopes2Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_ValidationScopes2);

                // we are testing the first button

                // don't fill required field, the client validation should appear
                browser.Single(".result").CheckIfTextEquals("0");
                browser.ElementAt("input[type=button]", 0).Click();
                browser.Single(".result").CheckIfTextEquals("0");
                browser.ElementAt("input[type=text]", 0).CheckIfHasClass("has-error");
                browser.ElementAt("input[type=text]", 1).CheckIfHasNotClass("has-error");

                // fill first required field and second field with a short string, the server validation should appear
                browser.ElementAt("input[type=text]", 0).SendKeys("aaa");
                browser.ElementAt("input[type=text]", 1).SendKeys("aaa");
                browser.Single(".result").CheckIfTextEquals("0");
                browser.ElementAt("input[type=button]", 0).Click();
                browser.Single(".result").CheckIfTextEquals("0");
                browser.ElementAt("input[type=text]", 0).CheckIfHasNotClass("has-error");
                browser.ElementAt("input[type=text]", 1).CheckIfHasClass("has-error");

                // fill the second field so the validation passes
                browser.ElementAt("input[type=text]", 1).Clear().SendKeys("aaaaaa");
                browser.Single(".result").CheckIfTextEquals("0");
                browser.ElementAt("input[type=button]", 0).Click();
                browser.Single(".result").CheckIfTextEquals("1");
                browser.ElementAt("input[type=text]", 0).CheckIfHasNotClass("has-error");
                browser.ElementAt("input[type=text]", 1).CheckIfHasNotClass("has-error");

                // clear the fields
                browser.ElementAt("input[type=text]", 0).Clear();
                browser.ElementAt("input[type=text]", 1).Clear();

                // we are testing the second button

                // don't fill required field, the client validation should appear
                browser.Single(".result").CheckIfTextEquals("1");
                browser.ElementAt("input[type=button]", 1).Click();
                browser.Single(".result").CheckIfTextEquals("1");
                browser.ElementAt("input[type=text]", 0).CheckIfHasClass("has-error");
                browser.ElementAt("input[type=text]", 1).CheckIfHasNotClass("has-error");

                // fill first required field and second field with a short string, the server validation should appear
                browser.ElementAt("input[type=text]", 0).SendKeys("aaa");
                browser.ElementAt("input[type=text]", 1).SendKeys("aaa");
                browser.Single(".result").CheckIfTextEquals("1");
                browser.ElementAt("input[type=button]", 1).Click();
                browser.Single(".result").CheckIfTextEquals("1");
                browser.ElementAt("input[type=text]", 0).CheckIfHasNotClass("has-error");
                browser.ElementAt("input[type=text]", 1).CheckIfHasClass("has-error");

                // fill the second field so the validation passes
                browser.ElementAt("input[type=text]", 1).Clear().SendKeys("aaaaaa");
                browser.Single(".result").CheckIfTextEquals("1");
                browser.ElementAt("input[type=button]", 1).Click();
                browser.Single(".result").CheckIfTextEquals("2");
                browser.ElementAt("input[type=text]", 0).CheckIfHasNotClass("has-error");
                browser.ElementAt("input[type=text]", 1).CheckIfHasNotClass("has-error");

            });
        }
    }
}
