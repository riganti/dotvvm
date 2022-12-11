using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;

namespace DotVVM.Samples.Tests.Complex;

public class ViewModelPopulateTests : AppSeleniumTest
{
    public ViewModelPopulateTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Complex_ViewModelPopulate_ViewModelPopulate()
    {
        RunInAllBrowsers(browser => {
            browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ViewModelPopulate_ViewModelPopulate);

            browser.Single("input[type=text]").Clear().SendKeys("test");
            browser.Single("input[type=button]").Click();
            AssertUI.Value(browser.Single("input[type=text]"), "testsuccess");
        });
    }
}
