using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class SPARedirectTests : SeleniumTestBase
    {
        [TestMethod]
        public void Complex_SPARedirect()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("ComplexSamples/SPARedirect");
                browser.Wait(1000);

                //check url
                browser.CheckUrl(s => s.Contains("ComplexSamples/SPARedirect/login?ReturnUrl="));
                browser.CheckUrl(s => !s.Contains("#"));

                // login to the app
                browser.First("input[type=button]").CheckAttribute("value", "Login").Click().Wait(1000);

                //check url
                browser.CheckUrl(s => s.Contains("ComplexSamples/SPARedirect#!/ComplexSamples/SPARedirect"));

                // sign out
                browser.First("input[type=button]").CheckAttribute("value", "Sign Out").Click().Wait(1000);
                
                //check url
                browser.CheckUrl(s => s.Contains("ComplexSamples/SPARedirect#!/ComplexSamples/SPARedirect?refresh=1"));

                // login to the app
                browser.First("input[type=button]").CheckAttribute("value", "Login").Click().Wait(1000);

                //check url
                browser.CheckUrl(s => s.Contains("ComplexSamples/SPARedirect#!/ComplexSamples/SPARedirect"));

                // sign out
                browser.First("input[type=button]").CheckAttribute("value", "Sign Out").Click().Wait(1000);

            });
        }
    }
}
