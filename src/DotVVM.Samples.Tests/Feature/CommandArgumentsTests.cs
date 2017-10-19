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
    public class CommandArgumentsTests : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_CommandArguments_CommandArguments()
        {
            const string Value = "testing value";

            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_CommandArguments_CommandArguments);

                var text = browser.Single("[data-ui='value']");
                text.CheckIfTextEquals("Nothing here");

                browser.Single("[data-ui='button'] button").Click();
                var alert = browser._GetInternalWebDriver().SwitchTo().Alert();
                alert.SendKeys(Value);
                alert.Accept();

                browser.Wait();
                text.CheckIfTextEquals(Value);
            });
        }
    }
}
