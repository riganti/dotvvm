using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;
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
                    browser.Wait();
                    browser.First("span[data-bind=\"text: Greeting\"]").CheckIfInnerTextEquals("Hello Deep Thought!");

                    browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand);
                    browser.Wait();

                    browser.Last("input[type=button]").Click();
                    browser.Wait();
                    browser.First("span[data-bind=\"text: Greeting\"]").CheckIfInnerTextEquals("Hello Deep Thought!");
                });
        }
    }
}
