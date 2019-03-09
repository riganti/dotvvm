using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    public class NamespaceCollisionTests : AppSeleniumTest
    {
        [Fact]
        public void Complex_NamespaceCollision_NamespaceCollision()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_NamespaceCollision_NamespaceCollision);
                AssertUI.TextEquals(browser.First("body"), "Hello from DotVVM!");
            });
        }

        public NamespaceCollisionTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
