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

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class ResourceLinksTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task OnlyLiteralInBody()
        {
            // There was a bug which collapsed the body contents into a `data-bind='text: ...'` binding
            var r = await cth.RunPageRaw("""
                @viewModel DotVVM.Framework.Tests.ControlTests.ResourceLinksTests.BasicTestViewModel
                <head></head>
                <body>
                    Id: {{value: Integer}}
                </body>
                """);

            check.CheckString(r.OutputString, fileExtension: "html");
        }
        [TestMethod]
        public async Task MultipleBodyElements()
        {
            // You can put body into the document by accident. In such case, DotVVM should try to select the outermost one
            var r = await cth.RunPageRaw("""
                @viewModel DotVVM.Framework.Tests.ControlTests.ResourceLinksTests.BasicTestViewModel
                <head></head>
                <body>
                    <table>
                        <body> {{value: Integer}} </body>
                    </table>
                </body>
                """);

            check.CheckString(r.OutputString, fileExtension: "html");
        }
        [TestMethod]
        public async Task NoHeadBodyElements()
        {
            // No body and head elements, DotVVM should put the resources at the start and end of the document,
            // the browser will hopefully correctly infer the head and body elements
            var r = await cth.RunPageRaw("""
                @viewModel DotVVM.Framework.Tests.ControlTests.ResourceLinksTests.BasicTestViewModel
                
                <div class-a={value: Integer >= 0}>{{value: Integer}}</div>
                """);

            check.CheckString(r.OutputString, fileExtension: "html");
        }
        public class BasicTestViewModel: DotvvmViewModelBase
        {
            [Bind(Name = "int")]
            public int Integer { get; set; } = 10;
        }
    }
}
