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
    public class BindingNamespacesTests : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_BindingNamespaces_BindingUsingNamespace()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_BindingNamespaces_BindingUsingNamespace);

                var dataUis = new[]
                {
                    "viewModel-namespace",
                    "fully-qualified-name",
                    "alias-in-config",
                    "import-in-config",
                    "import-directive"
                };

                foreach (var dataUi in dataUis)
                {
                    browser.Single($"[data-ui='{dataUi}']").CheckIfTextEquals("Works");
                }
            });
        }
    }
}
