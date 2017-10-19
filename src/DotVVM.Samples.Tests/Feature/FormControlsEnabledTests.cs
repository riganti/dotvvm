using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class FormControlsEnabledTests : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_FormControlsEnabled_FormControlsEnabled()
        {
            // Button, CheckBox, ComboBox, ListBox, RadioButton, TextBox
            string[] prefixes = { "b", "c", "cb", "lb", "rb", "tb" };

            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_FormControlsEnabled_FormControlsEnabled);

                bool enabled = false;

                for (int i = 0; i < 2; i++)
                {
                    foreach (var prefix in prefixes)
                    {
                        // These controls should always be enabled because they are explicitly set to Enabled
                        browser.First($"#{prefix}1-enabled").CheckIfIsEnabled();
                        browser.First($"#{prefix}2-enabled").CheckIfIsEnabled();
                        browser.First($"#repeater_0_{prefix}-enabled").CheckIfIsEnabled();
                        browser.First($"#repeater_1_{prefix}-enabled").CheckIfIsEnabled();

                        // These controls should always be disabled
                        browser.First($"#{prefix}1-disabled").CheckIfIsNotEnabled();
                        browser.First($"#{prefix}2-disabled").CheckIfIsNotEnabled();
                        browser.First($"#repeater_0_{prefix}-disabled").CheckIfIsNotEnabled();
                        browser.First($"#repeater_1_{prefix}-disabled").CheckIfIsNotEnabled();

                        // These should be changed by the Toggle button
                        if (enabled)
                        {
                            browser.First($"#{prefix}1-default").CheckIfIsEnabled();
                            browser.First($"#{prefix}2-default").CheckIfIsEnabled();
                        }
                        else
                        {
                            browser.First($"#{prefix}1-default").CheckIfIsNotEnabled();
                            browser.First($"#{prefix}2-default").CheckIfIsNotEnabled();
                        }

                        // These are overriden by the repeater
                        browser.First($"#repeater_0_{prefix}-default").CheckIfIsNotEnabled();
                        browser.First($"#repeater_1_{prefix}-default").CheckIfIsEnabled();
                    }
                    browser.First("#toggle").Click().Wait();
                    enabled = !enabled;
                }

                // LinkButton tests. Selenium does not recognize them as disabled as that is handled by DotVVM.
                int linkButtonPresses = 0;

                // These controls should always be enabled because they are explicitly set to Enabled
                TestLinkButton(browser, "linkb1-enabled", true, ref linkButtonPresses);
                TestLinkButton(browser, "linkb2-enabled", true, ref linkButtonPresses);
                TestLinkButton(browser, "repeater_0_linkb-enabled", true, ref linkButtonPresses);
                TestLinkButton(browser, "repeater_1_linkb-enabled", true, ref linkButtonPresses);

                // These controls should always be disabled
                TestLinkButton(browser, "linkb1-disabled", false, ref linkButtonPresses);
                TestLinkButton(browser, "linkb2-disabled", false, ref linkButtonPresses);
                TestLinkButton(browser, "repeater_0_linkb-disabled", false, ref linkButtonPresses);
                TestLinkButton(browser, "repeater_1_linkb-disabled", false, ref linkButtonPresses);

                // These should be changed by the Toggle button
                TestLinkButton(browser, "linkb1-default", enabled, ref linkButtonPresses);
                TestLinkButton(browser, "linkb2-default", enabled, ref linkButtonPresses);

                // These are overriden by the repeater
                TestLinkButton(browser, "repeater_0_linkb-default", false, ref linkButtonPresses);
                TestLinkButton(browser, "repeater_1_linkb-default", true, ref linkButtonPresses);

                browser.First("#toggle").Click().Wait();
                enabled = !enabled;
            });
        }

        private void TestLinkButton(BrowserWrapper browser, string id, bool shouldBeEnabled, ref int currentPresses)
        {
            browser.First($"#{id}").Click();
            if (shouldBeEnabled)
            {
                currentPresses++;
            }

            browser.First("#linkbuttons-pressed").CheckIfInnerTextEquals(currentPresses.ToString());
        }

    }
}
