using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Selenium.Core;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class DependencyInjectionTests : AppSeleniumTest
    {

        [TestMethod]
        public void Feature_DependencyInjection_ViewModelScopedService()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_DependencyInjection_ViewModelScopedService);

                for (int i = 0; i < 5; i++)
                {
                    var value = browser.First(".result").GetInnerText();
                    browser.First("input[type=button]").Click().Wait();
                    var value2 = browser.First(".result").GetInnerText();

                    Assert.AreNotEqual(value, value2);
                }
            });
        }

    }
}
