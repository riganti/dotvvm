using System.Linq;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class ClaimViewTests : SeleniumTestBase
    {
        [TestMethod]
        public void ClaimViewTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ClaimView_ClaimViewTest);

                // make sure we are signed out

                browser.First("input[value='Sign Out']").Click().Wait();

                browser.FindElements(".result1").ThrowIfDifferentCountThan(1).First().CheckIfInnerTextEquals("I am not a member!");
                browser.FindElements(".result2").ThrowIfDifferentCountThan(0);
                browser.FindElements(".result3").ThrowIfDifferentCountThan(0);

                // sign in as admin

                browser.First("input[type=checkbox][value=admin]").Click();
                browser.First("input[value='Sign In']").Click().Wait();

                browser.FindElements(".result1").ThrowIfDifferentCountThan(1).First().CheckIfInnerTextEquals("I am a member!");
                browser.FindElements(".result2").ThrowIfDifferentCountThan(1).First().CheckIfInnerTextEquals("I am a member!");
                browser.FindElements(".result3").ThrowIfDifferentCountThan(1).First().CheckIfInnerTextEquals("I am a member!");

                // sign in as moderator and headhunter

                browser.First("input[type=checkbox][value=moderator]").Click();
                browser.First("input[type=checkbox][value=headhunter]").Click();
                browser.First("input[value='Sign In']").Click().Wait();

                browser.FindElements(".result1").ThrowIfDifferentCountThan(1).First().CheckIfInnerTextEquals("I am a member!");
                browser.FindElements(".result2").ThrowIfDifferentCountThan(1).First().CheckIfInnerTextEquals("I am a member!");
                browser.FindElements(".result3").ThrowIfDifferentCountThan(1).First().CheckIfInnerTextEquals("I am a member!");

                // sign in as headhunter only

                browser.First("input[type=checkbox][value=headhunter]").Click();
                browser.First("input[value='Sign In']").Click().Wait();

                browser.FindElements(".result1").ThrowIfDifferentCountThan(1).First().CheckIfInnerTextEquals("I am not a member!");
                browser.FindElements(".result2").ThrowIfDifferentCountThan(0);
                browser.FindElements(".result3").ThrowIfDifferentCountThan(1).First().CheckIfInnerTextEquals("I am a member!");

                // sign in as tester only

                browser.First("input[type=checkbox][value=tester]").Click();
                browser.First("input[value='Sign In']").Click().Wait();

                browser.FindElements(".result1").ThrowIfDifferentCountThan(1).First().CheckIfInnerTextEquals("I am a member!");
                browser.FindElements(".result2").ThrowIfDifferentCountThan(1).First().CheckIfInnerTextEquals("I am a member!");
                browser.FindElements(".result3").ThrowIfDifferentCountThan(1).First().CheckIfInnerTextEquals("I am a member!");

                // sign out

                browser.First("input[value='Sign Out']").Click().Wait();

                browser.FindElements(".result1").ThrowIfDifferentCountThan(1).First().CheckIfInnerTextEquals("I am not a member!");
                browser.FindElements(".result2").ThrowIfDifferentCountThan(0);
                browser.FindElements(".result3").ThrowIfDifferentCountThan(0);
            });
        }
    }
}