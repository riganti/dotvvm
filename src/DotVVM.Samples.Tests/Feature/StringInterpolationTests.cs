using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class StringInterpolationTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_StringInterpolation_SpecialCharacterTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StringInterpolation);
            });


        }
        public StringInterpolationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
