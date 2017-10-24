using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ViewModelDeserializationTests : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_ViewModelDeserialization_DoesNotDropObject()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ViewModelDeserialization_DoesNotDropObject);


                browser.First("span").CheckIfInnerTextEquals("0");
                //value++
                browser.ElementAt("input[type=button]",2).Click();
                browser.ElementAt("input[type=button]", 2).Click();
                //check value
                browser.First("span").CheckIfInnerTextEquals("2");
                //hide span
                browser.ElementAt("input[type=button]", 0).Click();
                //show span
                browser.ElementAt("input[type=button]", 1).Click();
                //value++
                browser.ElementAt("input[type=button]", 2).Click();
                //check value
                browser.First("span").CheckIfInnerTextEquals("3");
               
            });
        }
    }
}
