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
    public class Validation: SeleniumTestBase
    {
        [TestMethod]
        public void ValidationTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Validation_Validationn);

                browser.First("input[type=button]").Click();

                browser.First("li").CheckIfInnerText(i => i.Contains("The Text field is required."));

            });
        }
    }
}
