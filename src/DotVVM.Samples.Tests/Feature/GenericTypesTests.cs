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
    public class GenericTypesTests : SeleniumTestBase
    {
        [TestMethod]
        public void GenericTypes_TypedInResourceBinding()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_GenericTypes_InResourceBinding);

                browser
                    .Single("span[data-ui=generic-instance-function]")
                    .CheckIfInnerTextEquals("Hello from instance generic method arg1:Hallo from generic parameter. arg2:Hallo from generic parameter.");

                browser
                    .Single("span[data-ui=generic-class-full]")
                    .CheckIfInnerTextEquals("Hallo from generic parameter.");

                browser
                    .Single("span[data-ui=generic-class-aliased]")
                    .CheckIfInnerTextEquals("Hallo from generic parameter.");

                browser
                    .Single("span[data-ui=generic-static-function-aliased]")
                    .CheckIfInnerTextEquals("Hello from static generic method arg1:Hallo from generic parameter. arg2:Hallo from generic parameter.");
            });
        }

        [TestMethod]
        public void GenericTypes_InstanceCommand_StaticCommand()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_GenericTypes_InCommandBinding);

                browser.Single("input[data-ui=static]").Click();
                browser.Single("input[data-ui=instance]").Click();

                browser
                    .Single("span[data-ui=output]")
                    .CheckIfInnerTextEquals("Hello from instance generic command arg1:Hallo from generic parameter. arg2:Hallo from generic parameter.");

                browser
                    .Single("span[data-ui=static-output]")
                    .CheckIfInnerTextEquals("Hello from static generic command arg1:Hallo from generic parameter. arg2:Hallo from generic parameter.");
            });
        }
    }
}
