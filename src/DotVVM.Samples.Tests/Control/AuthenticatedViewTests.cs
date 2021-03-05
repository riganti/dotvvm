using Riganti.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class AuthenticatedViewTests : AppSeleniumTest
    {

        [Fact]
        public void Control_AuthenticatedView_AuthenticatedViewTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_AuthenticatedView_AuthenticatedViewTest);

                // make sure we are signed out
                browser.First("input[value='Sign Out']").Click().Wait();

                AssertUI.InnerTextEquals(browser.First(".result"), "I am not authenticated!");
                browser.First("input[value='Sign In']").Click().Wait();
                AssertUI.InnerTextEquals(browser.First(".result"), "I am authenticated!");
                browser.First("input[value='Sign Out']").Click().Wait();
                AssertUI.InnerTextEquals(browser.First(".result"), "I am not authenticated!");
            });
        }

        public AuthenticatedViewTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
