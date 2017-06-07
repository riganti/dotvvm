using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class ServerCommentsTests : SeleniumTest
    {
        [TestMethod]
        public void ServerComments()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerComments_ServerComments);

                browser.Single("#before").CheckIfInnerText(s => s.Contains("Before Server"));
                browser.Single("#afterFirst").CheckIfInnerText(s => s.Contains("After Server"));
                browser.Single("#afterOther").CheckIfInnerText(s => s.Contains("After Other"));
                browser.FindElements("#firstHidden").ThrowIfDifferentCountThan(0);
                browser.FindElements("#otherHidden").ThrowIfDifferentCountThan(0);
                
            });
        }
    }
}
