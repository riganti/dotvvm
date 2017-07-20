using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class DependencyInjectionTests : SeleniumTest
    {
        [TestMethod]
        public void Feature_DependencyInjection_ViewModelScopedService()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_DependencyInjection_ViewModelScopedService);

                var initialValue = Convert.ToInt32(browser.Single("[data-ui='id-span']").GetText());
                var button = browser.Single("[data-ui='postback-button']");
                for (int i = 0; i < 5; i++)
                {
                    button.Click();
                    var value = Convert.ToInt32(browser.Single("[data-ui='id-span']").GetText());
                    Assert.IsTrue(value > initialValue);
                }
            });
        }

    }
}
