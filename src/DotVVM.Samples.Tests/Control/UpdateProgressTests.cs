using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class UpdateProgressTests : SeleniumTestBase
    {

        [TestMethod]
        public void Control_UpdateProgress()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_UpdateProgress_UpdateProgress);
                browser.Wait();

                // click the button and verify that the progress appears and disappears again
                browser.First(".update-progress").CheckIfIsNotDisplayed();
                browser.ElementAt("input[type=button]", 0).Click();
                browser.First(".update-progress").CheckIfIsDisplayed();
                browser.Wait(3000);
                browser.First(".update-progress").CheckIfIsNotDisplayed();

                // click the second button and verify that the progress appears and disappears again
                browser.First(".update-progress").CheckIfIsNotDisplayed();
                browser.ElementAt("input[type=button]", 1).Click();
                browser.Wait(1000);
                browser.First(".update-progress").CheckIfIsNotDisplayed();
            });
        }


    }
}