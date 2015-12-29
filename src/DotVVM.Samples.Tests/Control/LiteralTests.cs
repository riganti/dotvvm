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
    public class LiteralTests : SeleniumTestBase
    {

        [TestMethod]
        public void Control_Literal()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Literal_Literal);

                foreach (var column in browser.FindElements("td"))
                {
                    column.ElementAt("fieldset", 0).Single("span").CheckIfInnerTextEquals("Hardcoded value");
                    column.ElementAt("fieldset", 1).Single("span").CheckIfInnerTextEquals("Hello");
                    column.ElementAt("fieldset", 2).Single("span").CheckIfInnerTextEquals("1/1/2000");

                    column.ElementAt("fieldset", 3).FindElements("span").ThrowIfDifferentCountThan(0);
                    column.ElementAt("fieldset", 3).CheckIfInnerText(t => t.Contains("Hardcoded value"));

                    column.ElementAt("fieldset", 4).FindElements("span").ThrowIfDifferentCountThan(0);
                    column.ElementAt("fieldset", 4).CheckIfInnerText(t => t.Contains("Hello"));

                    column.ElementAt("fieldset", 5).FindElements("span").ThrowIfDifferentCountThan(0);
                    column.ElementAt("fieldset", 5).CheckIfInnerText(t => t.Contains("1/1/2000"));
                }
            });
        }
        [TestMethod]
        public void Control_Literal_FormatString()
        {
            RunInAllBrowsers(browser =>
            {
                Action<string> checkFormat = (string format) =>
                {
                    //check format d
                    var text = browser.First("#results-" + format).GetText();
                    browser.First("#client-format-" + format).CheckIfInnerTextEquals(text, false);
                    browser.First("#server-render-format-" + format).CheckIfInnerTextEquals(text, false);

                };
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Literal_Literal_FormatString);
                browser.First("#change-culture").Click();

                checkFormat("d");
                checkFormat("D");
                //dd
            });
        }


    }
}