using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class ServerCommentsTests : SeleniumTestBase
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
                if (!browser.FindElements("#firstHidden").Any())
                { 
                }
                else
                {
                    throw new Exception("Hidden element found!");
                }

                if (!browser.FindElements("#otherHidden").Any())
                {
                }
                else
                {
                    throw new Exception("Hidden element found!");
                }

            });
        }
    }
}
