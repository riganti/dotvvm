using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class FormControlsEnabledTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_FormControlsEnabled_FormControlsEnabled()
        {
            // Button, CheckBox, ComboBox, ListBox, RadioButton, TextBox
            string[] prefixes = { "b", "c", "cb", "lb", "rb", "tb" };

            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_FormControlsEnabled_FormControlsEnabled);
                browser.Wait(2000);

                // LinkButton tests. Selenium does not recognize them as disabled as that is handled by DotVVM.
                int linkButtonPresses = 0;
                bool enabled = false;

                for (int i = 0; i < 2; i++)
                {
                    foreach (var prefix in prefixes)
                    {
                        // These controls should always be enabled because they are explicitly set to Enabled
                        AssertUI.IsEnabled(browser.First($"#{prefix}1-enabled"));
                        AssertUI.IsEnabled(browser.First($"#{prefix}2-enabled"));
                        AssertUI.IsEnabled(browser.First($"#repeater_0_{prefix}-enabled"));
                        AssertUI.IsEnabled(browser.First($"#repeater_1_{prefix}-enabled"));

                        // These controls should always be disabled
                        AssertUI.IsNotEnabled(browser.First($"#{prefix}1-disabled"));
                        AssertUI.IsNotEnabled(browser.First($"#{prefix}2-disabled"));
                        AssertUI.IsNotEnabled(browser.First($"#repeater_0_{prefix}-disabled"));
                        AssertUI.IsNotEnabled(browser.First($"#repeater_1_{prefix}-disabled"));

                        // These should be changed by the Toggle button
                        if (enabled)
                        {
                            AssertUI.IsEnabled(browser.First($"#{prefix}1-default"));
                            AssertUI.IsEnabled(browser.First($"#{prefix}2-default"));
                        }
                        else
                        {
                            AssertUI.IsNotEnabled(browser.First($"#{prefix}1-default"));
                            AssertUI.IsNotEnabled(browser.First($"#{prefix}2-default"));
                        }

                        // These are overriden by the repeater
                        AssertUI.IsNotEnabled(browser.First($"#repeater_0_{prefix}-default"));
                        AssertUI.IsEnabled(browser.First($"#repeater_1_{prefix}-default"));
                    }

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

                    browser.First("#toggle").Click();
                    enabled = !enabled;
                }
            });
        }

        private void TestLinkButton(IBrowserWrapper browser, string id, bool shouldBeEnabled, ref int currentPresses)
        {
            browser.First($"#{id}").Click();
            if (shouldBeEnabled)
            {
                currentPresses++;
            }

            var c = currentPresses;
            browser.WaitFor(() => {
                AssertUI.InnerTextEquals(browser.First("#linkbuttons-pressed"), c.ToString());
            }, 2000);
        }

        public FormControlsEnabledTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
