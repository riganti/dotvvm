using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class ValidationErrorsCountTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper();
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task ValidationErrorsCount_BasicRendering()
        {
            var r = await cth.RunPage(typeof(TestViewModel), @"
                @viewModel DotVVM.Framework.Tests.ControlTests.ValidationErrorsCountTests.TestViewModel
                <!-- default settings -->
                <dot:ValidationErrorsCount />
                <!-- with validation target -->
                <dot:ValidationErrorsCount Validation.Target={value: Person} />
                <!-- with InvalidCssClass -->
                <dot:ValidationErrorsCount InvalidCssClass='has-errors' />
                <!-- with custom wrapper tag -->
                <dot:ValidationErrorsCount WrapperTagName='div' />
                <!-- not including errors from children -->
                <dot:ValidationErrorsCount IncludeErrorsFromChildren=false />
                <!-- not including errors from target -->
                <dot:ValidationErrorsCount IncludeErrorsFromTarget=false />
            ");

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task ValidationErrorsCount_WithValidationDisabled()
        {
            var r = await cth.RunPage(typeof(TestViewModel), @"
                @viewModel DotVVM.Framework.Tests.ControlTests.ValidationErrorsCountTests.TestViewModel
                <!-- validation disabled -->
                <dot:ValidationErrorsCount Validation.Enabled=false />
            ");

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        public class TestViewModel : DotvvmViewModelBase
        {
            public PersonModel Person { get; set; } = new PersonModel();
        }

        public class PersonModel
        {
            [Required]
            public string? Name { get; set; }

            [Required]
            [EmailAddress]
            public string? Email { get; set; }
        }
    }
}
