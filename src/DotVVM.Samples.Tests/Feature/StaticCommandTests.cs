using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class StaticCommandTests : SeleniumTestBase
    {
        [TestMethod]
        public void Feature_StaticCommand()
        {
            RunInAllBrowsers(browser =>
                {
                    browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand);
                    browser.Wait();

                    browser.First("input[type=button]").Click();
                    browser.First("span").CheckIfInnerTextEquals("Hello Deep Thought!");

                    browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand);
                    browser.Wait();

                    browser.Last("input[type=button]").Click();
                    browser.First("span").CheckIfInnerTextEquals("Hello Deep Thought!");
                });
        }
    }
}
