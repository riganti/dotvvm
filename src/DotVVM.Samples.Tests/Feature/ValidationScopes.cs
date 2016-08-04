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
    }
}
