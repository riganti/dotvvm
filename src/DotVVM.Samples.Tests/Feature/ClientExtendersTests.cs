using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ClientExtendersTests : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_ClientExtenders_PasswordStrength()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ClientExtenders_PasswordStrength);

                var message = browser.Single("[data-ui='strength-message']");
                message.CheckIfTextEquals("Enter password");

                var textBox = browser.Single("[data-ui='password-textBox']");
                textBox.SendKeys("password");

                message.CheckIfTextEquals("Good");
            });
        }
    }
}
