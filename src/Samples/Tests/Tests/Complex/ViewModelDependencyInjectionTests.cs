using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;

namespace DotVVM.Samples.Tests.Complex
{
    public class ViewModelDependencyInjectionTests : AppSeleniumTest
    {
        public ViewModelDependencyInjectionTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Complex_ViewModelDependencyInjection_Sample()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ViewModelDependencyInjection_Sample);

                browser.Single("a").Click();
                AssertUI.TextEquals(browser.Single(".result"), "true");
            });
        }
    }
}
