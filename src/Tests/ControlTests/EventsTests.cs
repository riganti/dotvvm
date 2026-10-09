using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class EventsTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper();

        [TestMethod]
        public async Task Key_RendersDocumentAndControlBindings()
        {
            var result = await cth.RunPage(typeof(TestViewModel), """
                <dot:Events Key-Ctrl+S="{command: Save()}"
                            Events.KeyEnabled-Ctrl+S="{value: Enabled}" />
                <div Events.Key-Shift+Alt+Enter="{command: Save()}"
                     Events.KeyEnabled-Shift+Alt+Enter="{value: Enabled}" />
                """);

            StringAssert.Contains(result.OutputString, "<!-- ko dotvvm-command-shortcut:");
            StringAssert.Contains(result.OutputString, "key: \"S\", ctrl: true, alt: false, shift: false, enabled: Enabled");
            StringAssert.Contains(result.OutputString, "data-bind='dotvvm-command-shortcut:");
            StringAssert.Contains(result.OutputString, "key: \"Enter\", ctrl: false, alt: true, shift: true, enabled: Enabled");
        }

        [DataTestMethod]
        [DataRow("Ctrl+Ctrl+S")]
        [DataRow("Ctrl+S+Alt")]
        [DataRow("Ctrl++S")]
        [DataRow("Control+S")]
        [DataRow("Ctrl+Unknown")]
        public async Task Key_RejectsInvalidShortcut(string shortcut)
        {
            var exception = await Assert.ThrowsExceptionAsync<DotvvmCompilationException>(() =>
                cth.RunPage(typeof(TestViewModel), $$"""
                    <dot:Events Key-{{shortcut}}="{command: Save()}" />
                    """, fileName: $"InvalidShortcut-{shortcut}.dothtml"));

            StringAssert.Contains(exception.Message, "must contain each of Ctrl, Alt and Shift at most once");
        }

        [TestMethod]
        public async Task KeyEnabled_RequiresMatchingKey()
        {
            var exception = await Assert.ThrowsExceptionAsync<DotvvmCompilationException>(() =>
                cth.RunPage(typeof(TestViewModel), """
                    <div Events.Key-Ctrl+S="{command: Save()}"
                         Events.KeyEnabled-Ctrl+Enter="{value: Enabled}" />
                    """));

            StringAssert.Contains(exception.Message, "can only be used together with Key-Ctrl+Enter");
        }

        public class TestViewModel
        {
            public bool Enabled { get; set; }
            public void Save() { }
        }
    }
}
