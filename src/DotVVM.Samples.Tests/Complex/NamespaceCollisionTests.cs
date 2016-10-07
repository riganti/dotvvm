using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class NamespaceCollisionTests : SeleniumTestBase
    {
        [TestMethod]
        public void NamespaceCollision()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_NamespaceCollision_NamespaceCollision);
                browser.First("body").CheckIfTextEquals("Hello from DotVVM!");
            });

        }
    }
}
