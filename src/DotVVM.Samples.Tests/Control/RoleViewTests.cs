using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class RoleViewTests : SeleniumTest
    {

        [TestMethod]
        public void Control_RoleView_RoleViewTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_RoleView_RoleViewTest);

                // make sure we are signed out (first should show IfNotMember, second should be hidden)
                browser.First("input[value='Sign Out']").Click().Wait();

                browser.FindElements(".result1").ThrowIfDifferentCountThan(1)
                    .First().CheckIfInnerTextEquals("I am not a member!");
                browser.FindElements(".result2").ThrowIfDifferentCountThan(0);

                // sign in as admin (both should show IsMember content)
                browser.First("input[type=checkbox][value=admin]").Click();
                browser.First("input[value='Sign In']").Click().Wait();

                browser.FindElements(".result1").ThrowIfDifferentCountThan(1)
                    .First().CheckIfInnerTextEquals("I am a member!");
                browser.FindElements(".result2").ThrowIfDifferentCountThan(1)
                    .First().CheckIfInnerTextEquals("I am a member!");

                // sign in as moderator and headhunter (both should show IsMember content)
                browser.First("input[type=checkbox][value=moderator]").Click();
                browser.First("input[type=checkbox][value=headhunter]").Click();
                browser.First("input[value='Sign In']").Click().Wait();

                browser.FindElements(".result1").ThrowIfDifferentCountThan(1)
                    .First().CheckIfInnerTextEquals("I am a member!");
                browser.FindElements(".result2").ThrowIfDifferentCountThan(1)
                    .First().CheckIfInnerTextEquals("I am a member!");

                // sign in as headhunter only (both should be visible but show that user is not a member)
                browser.First("input[type=checkbox][value=headhunter]").Click();
                browser.First("input[value='Sign In']").Click().Wait();

                browser.FindElements(".result1").ThrowIfDifferentCountThan(1)
                    .First().CheckIfInnerTextEquals("I am not a member!");
                browser.FindElements(".result2").ThrowIfDifferentCountThan(1)
                    .First().CheckIfInnerTextEquals("I am not a member!");

                // sign in as tester only (both should show IsMember content)
                browser.First("input[type=checkbox][value=tester]").Click();
                browser.First("input[value='Sign In']").Click().Wait();

                browser.FindElements(".result1").ThrowIfDifferentCountThan(1)
                    .First().CheckIfInnerTextEquals("I am a member!");
                browser.FindElements(".result2").ThrowIfDifferentCountThan(1)
                    .First().CheckIfInnerTextEquals("I am a member!");

                // sign out (first should show IfNotMember, second should be hidden)
                browser.First("input[value='Sign Out']").Click().Wait();

                browser.FindElements(".result1").ThrowIfDifferentCountThan(1)
                    .First().CheckIfInnerTextEquals("I am not a member!");
                browser.FindElements(".result2").ThrowIfDifferentCountThan(0);
            });
        }

    }
}
