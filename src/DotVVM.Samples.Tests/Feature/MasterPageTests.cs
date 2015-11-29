using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using Riganti.Utils.Testing.SeleniumCore;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class MasterPageTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_TwoNestedMasterPages()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_NestedMasterPages_Content);
                browser.First("h1"); // root masterpage
                browser.First("h2"); // nested masterpage
                browser.First("h3"); // nested page
            });


        }

    }
}
