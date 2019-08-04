using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class BindingNamespacesTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_BindingNamespaces_BindingUsingNamespace()
        {
            RunInAllBrowsers(browser => {
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
                    AssertUI.TextEquals(browser.Single($"[data-ui='{dataUi}']"), "Works");
                }
            });
        }

        public BindingNamespacesTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
