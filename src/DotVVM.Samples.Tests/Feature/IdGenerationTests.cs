using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class IdGenerationTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_IdGeneration_IdGeneration()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_IdGeneration_IdGeneration);
                

                AssertUI.Attribute(browser.Single("*[data-id=test1_marker]"), "id", s => s.Equals("test1"),
                         "Wrong ID");
                AssertUI.Attribute(browser.Single("*[data-id=test2_marker]"), "id", s => s.Equals("test2"),
                         "Wrong ID");

                AssertUI.Attribute(browser.Single("*[data-id=test1a_marker]"), "id", s => s.Equals("test1a"),
                         "Wrong ID");
                AssertUI.Attribute(browser.Single("*[data-id=test2a_marker]"), "id", s => s.Equals("test2a"),
                         "Wrong ID");

                var control1 = browser.Single("#ctl1");
                AssertUI.Attribute(control1.Single("*[data-id=control1_marker]"), "id", s => s.Equals("ctl1_control1"),
                    "Wrong ID");
                AssertUI.Attribute(control1.Single("*[data-id=control2_marker]"), "id", s => s.Equals("ctl1_control2"),
                    "Wrong ID");

                var control2 = browser.Single("#ctl2");
                AssertUI.Attribute(control2.Single("*[data-id=control1_marker]"), "id", s => s.Equals("control1"),
                    "Wrong ID");
                AssertUI.Attribute(control2.Single("*[data-id=control2_marker]"), "id", s => s.Equals("control2"),
                    "Wrong ID");

                var repeater1 = browser.Single("*[data-id=repeater1]");
                for (int i = 0; i < 4; i++)
                {
                    AssertUI.Attribute(repeater1.ElementAt("*[data-id=repeater1_marker]", i), "id",
                        s => s.Equals(repeater1.GetAttribute("id") + "_" + i + "_repeater1"), "Wrong ID");
                    AssertUI.Attribute(repeater1.ElementAt("*[data-id=repeater2_marker]", i), "id",
                        s => s.Equals(repeater1.GetAttribute("id") + "_" + i + "_repeater2"), "Wrong ID");
                }

                var repeater2 = browser.Single("*[data-id=repeater2]");
                for (int i = 0; i < 4; i++)
                {
                    AssertUI.Attribute(repeater2.ElementAt("*[data-id=repeater1server_marker]", i), "id",
                        s => s.Equals(repeater2.GetAttribute("id") + "_" + i + "_repeater1server"), "Wrong ID");
                    AssertUI.Attribute(repeater2.ElementAt("*[data-id=repeater2server_marker]", i), "id",
                        s => s.Equals(repeater2.GetAttribute("id") + "_" + i + "_repeater2server"), "Wrong ID");
                }

                foreach (var span in browser.Single("*[data-ui=repeater3]").Children)
                {
                    AssertUI.Attribute(span, "id", s => s.Equals(span.GetAttribute("data-ui")), "Wrong ID");
                }
            });
        }

        public IdGenerationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
