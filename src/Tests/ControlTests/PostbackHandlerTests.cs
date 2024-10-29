using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Testing;
using System.Security.Claims;
using System.Collections;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class PostbackHandlerTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
        });
        readonly OutputChecker check = new OutputChecker("testoutputs");


        [TestMethod]
        public async Task ButtonHandlers()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), """
                <!-- suppress postback -->
                <dot:Button DataContext={value: Nested} Click={staticCommand: 0} Text="Test supress">
                    <Postback.Handlers>
                        <dot:SuppressPostBackHandler Suppress={value: _parent.Integer > 100} />
                        <dot:SuppressPostBackHandler Suppress={value: SomeString.Length < 5} />
                    </Postback.Handlers>
                </dot:Button>

                <!-- confirm -->
                <dot:Button DataContext={value: Nested} Click={staticCommand: 0} Text="Test confirm">
                    <Postback.Handlers>
                        <dot:ConfirmPostBackHandler Message={value: $"String={_root.String} SomeString={SomeString}"} />
                    </Postback.Handlers>
                </dot:Button>
                """
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        public class BasicTestViewModel: DotvvmViewModelBase
        {
            public int Integer { get; set; } = 123;
            public bool Boolean { get; set; } = false;
            public string String { get; set; } = "some-string";

            public TestViewModel3 Nested { get; set; } = new TestViewModel3 { SomeString = "a" };
        }
    }
}
