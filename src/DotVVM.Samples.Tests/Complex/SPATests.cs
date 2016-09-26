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
    public class SPATests : SeleniumTestBase
    {
        [TestMethod]
        public void Complex_SPA()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("/");
                browser.Wait(1000);

                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_SPA_default);
                browser.Wait(1000);

                // go to test page
                browser.Single("h2").CheckIfTextEquals("Default");
                browser.ElementAt("a", 1).Click().Wait();

                // check url and page contents
                browser.Single("h2").CheckIfTextEquals("Test");
                browser.CheckUrl(s => s.Contains("ComplexSamples/SPA/default#!/ComplexSamples/SPA/test"));

                // use the back button
                browser.NavigateBack();
                browser.Wait(1000);

                // check url and page contents
                browser.Single("h2").CheckIfTextEquals("Default");
                browser.CheckUrl(s => s.Contains("ComplexSamples/SPA/default"));
                
                // exit SPA using the back button
                browser.NavigateBack();
                browser.Wait(1000);

                // reenter SPA
                browser.NavigateForward();
                browser.Wait(1000);

                // check url and page contents
                browser.Single("h2").CheckIfTextEquals("Default");
                browser.CheckUrl(s => s.Contains("ComplexSamples/SPA/default"));

                // go forward to the test page
                browser.NavigateForward();
                browser.Wait(1000);

                // check url and page contents
                browser.Single("h2").CheckIfTextEquals("Test");
                browser.CheckUrl(s => s.Contains("ComplexSamples/SPA/default#!/ComplexSamples/SPA/test"));

                // open the default page
                browser.ElementAt("a", 0).Click().Wait();

                // check url and page contents
                browser.Single("h2").CheckIfTextEquals("Default");
                browser.CheckUrl(s => s.Contains("ComplexSamples/SPA/default#!/ComplexSamples/SPA/default"));

                // go back to the test page
                browser.NavigateBack();
                browser.Wait(1000);

                // check url and page contents
                browser.Single("h2").CheckIfTextEquals("Test");
                browser.CheckUrl(s => s.Contains("ComplexSamples/SPA/default#!/ComplexSamples/SPA/test"));
                
                // go back to the default page
                browser.NavigateBack();
                browser.Wait(1000);

                // check url and page contents
                browser.Single("h2").CheckIfTextEquals("Default");
                browser.CheckUrl(s => s.Contains("ComplexSamples/SPA/default"));
            });
        }
    }
}
