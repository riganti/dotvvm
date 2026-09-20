using System.Threading.Tasks;
using DotVVM.Framework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class CommandShortcutTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper();

        [TestMethod]
        public async Task CommandShortcut_RendersDocumentAndTargetedBindings()
        {
            var result = await cth.RunPage(typeof(TestViewModel), """
                <div ID="editor">
                    <dot:TextBox Text="{value: Label}" />
                </div>
                <dot:CommandShortcut Key="Escape"
                                     Command="{command: Label = null}" />
                <dot:CommandShortcut Key="Enter"
                                     Ctrl="true"
                                     Enabled="{value: Integer > 0}"
                                     TargetControlID="editor"
                                     Command="{command: Label = null}" />
                """);

            StringAssert.Contains(result.OutputString, "dotvvm-command-shortcut");
            StringAssert.Contains(result.OutputString, "key: \"Escape\"");
            StringAssert.Contains(result.OutputString, "key: \"Enter\"");
            StringAssert.Contains(result.OutputString, "ctrl: true");
            StringAssert.Contains(result.OutputString, "targetId: \"editor\"");
        }

        public class TestViewModel
        {
            public string Label { get; set; }
            public int Integer { get; set; }
        }
    }
}
