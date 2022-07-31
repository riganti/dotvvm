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

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class CompilerEdgeCaseTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
            config.Markup.AddCodeControls("cc", typeof(CompilerEdgeCaseTests));
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task ConstructorWithDefaultArgs()
        {
            await cth.RunPage(typeof(BasicTestViewModel), @"
                <cc:ControlConstructorWithDefaultArgs />
                <cc:ControlConstructorWithDefaultArgs2 /> ");

        }

        [TestMethod]
        public async Task EmptySeparatorTemplate()
        {
            // empty templates didn't work on .NET framework
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <dot:Repeater DataSource={value: Label}>
                    <SeparatorTemplate></SeparatorTemplate>
                    teststring
                </dot:Repeater> ");
        }

        public class BasicTestViewModel: DotvvmViewModelBase
        {
            public int Integer { get; set; } = 10000000;
            public double Float { get; set; } = 0.11111;
            public DateTime DateTime { get; set; } = DateTime.Parse("2020-08-11T16:01:44.5141480");
            public string Label { get; } = "My Label";
        }
    }

    public class ControlConstructorWithDefaultArgs : DotvvmControl
    {
        public ControlConstructorWithDefaultArgs(Func<int, string> a = null, int? b = null)
        {
            Assert.IsNull(a);
            Assert.IsNull(b);
        }
    }

    public class ControlConstructorWithDefaultArgs2 : DotvvmControl
    {
        public ControlConstructorWithDefaultArgs2(string a = "test", int? b = 12)
        {
            Assert.AreEqual("test", a);
            Assert.AreEqual(12, b);
        }
    }

}
