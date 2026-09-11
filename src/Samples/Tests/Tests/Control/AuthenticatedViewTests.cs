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
using static DotVVM.Samples.Tests.UITestUtils;

namespace DotVVM.Samples.Tests.Control
{
    public class AuthenticatedViewTests : AppSeleniumTest
    {

        [Fact]
        public void Control_AuthenticatedView_AuthenticatedViewTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_AuthenticatedView_AuthenticatedViewTest);

                void AssertAuthenticationState(string expectedResult)
                {
                    WaitForIgnoringStaleElements(() => {
                        AssertUI.InnerTextEquals(browser.First(".result"), expectedResult);
                    });
                }

                // make sure we are signed out
                browser.First("input[value='Sign Out']").Click();
                AssertAuthenticationState("I am not authenticated!");
                browser.First("input[value='Sign In']").Click();
                AssertAuthenticationState("I am authenticated!");
                browser.First("input[value='Sign Out']").Click();
                AssertAuthenticationState("I am not authenticated!");
            });
        }

        public AuthenticatedViewTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
