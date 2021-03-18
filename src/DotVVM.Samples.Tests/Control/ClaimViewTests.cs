using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;

namespace DotVVM.Samples.Tests.Control
{
    public class ClaimViewTests : AppSeleniumTest
    {
        public ClaimViewTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Control_ClaimView_ClaimViewTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_ClaimView_ClaimViewTest);

                void AssertInnerTextEquals(string selector, string text)
                {
                    browser.WaitFor(() => {
                        AssertUI.InnerTextEquals(
                            browser.FindElements(selector).ThrowIfDifferentCountThan(1).First(),
                            text);
                    }, 5000);
                }

                // make sure we are signed out (first should show IfNotMember, second should be hidden)
                browser.First("input[value='Sign Out']").Click();

                AssertInnerTextEquals(".result1", "I am not a member!");
                AssertUI.IsNotDisplayed(browser, ".result2");
                AssertUI.IsNotDisplayed(browser, ".result3");

                // sign in as admin (both should show IsMember content)
                browser.First("input[type=checkbox][value=admin]").Click();
                browser.First("input[value='Sign In']").Click();

                AssertInnerTextEquals(".result1", "I am a member!");
                AssertInnerTextEquals(".result2", "I am a member!");
                AssertInnerTextEquals(".result3", "I am a member!");

                // sign in as moderator and headhunter (both should show IsMember content)
                browser.First("input[type=checkbox][value=moderator]").Click();
                browser.First("input[type=checkbox][value=headhunter]").Click();
                browser.First("input[value='Sign In']").Click();

                AssertInnerTextEquals(".result1", "I am a member!");
                AssertInnerTextEquals(".result2", "I am a member!");
                AssertInnerTextEquals(".result3", "I am a member!");

                // sign in as headhunter only (both should be visible but show that user is not a member)
                browser.First("input[type=checkbox][value=headhunter]").Click();
                browser.First("input[value='Sign In']").Click();

                AssertInnerTextEquals(".result1", "I am not a member!");
                AssertInnerTextEquals(".result2", "I am not a member!");
                AssertInnerTextEquals(".result3", "I am a member!");

                // sign in as tester only (both should show IsMember content)
                browser.First("input[type=checkbox][value=tester]").Click();
                browser.First("input[value='Sign In']").Click();

                AssertInnerTextEquals(".result1", "I am a member!");
                AssertInnerTextEquals(".result2", "I am a member!");
                AssertInnerTextEquals(".result3", "I am a member!");

                // sign out (first should show IfNotMember, second should be hidden)
                browser.First("input[value='Sign Out']").Click();

                AssertInnerTextEquals(".result1", "I am not a member!");
                AssertUI.IsNotDisplayed(browser, ".result2");
                AssertUI.IsNotDisplayed(browser, ".result3");
            });
        }
    }
}
