using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    public class DataTemplateTests : AppSeleniumTest
    {
        [Fact]
        public void Complex_EmptyDataTemplate_RepeaterGridView()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_EmptyDataTemplate_RepeaterGridView);
                
                void isDisplayed(string id) => AssertUI.IsDisplayed(browser, "#" + id);
                void isHidden(string id) => AssertUI.IsNotDisplayed(browser, "#" + id);
                void isNotPresent(string id) => browser.FindElements("#" + id + " > *").ThrowIfDifferentCountThan(0);

                isHidden("marker1_parent");
                isDisplayed("marker1");

                isNotPresent("marker2_parent");
                isDisplayed("marker2");

                isHidden("marker3_parent");
                isDisplayed("marker3");

                isNotPresent("marker4_parent");
                isDisplayed("marker4");

                isDisplayed("nonempty_marker1_parent");
                isHidden("nonempty_marker1");

                isDisplayed("nonempty_marker2_parent");
                isNotPresent("nonempty_marker2");

                isDisplayed("nonempty_marker3_parent");
                isHidden("nonempty_marker3");

                isDisplayed("nonempty_marker4_parent");
                isNotPresent("nonempty_marker4");

                isHidden("null_marker1_parent");
                isDisplayed("null_marker1");

                isNotPresent("null_marker2_parent");
                isDisplayed("null_marker2");

                isHidden("null_marker3_parent");
                isDisplayed("null_marker3");

                isNotPresent("null_marker4_parent");
                isDisplayed("null_marker4");
            });
        }

        public DataTemplateTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
