using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.ViewModel
{
    [TestClass]
    public class ModelStateFromExpressionTests
    {
        private readonly ViewModel viewModel;

        public ModelStateFromExpressionTests()
        {
            this.viewModel = new ViewModel();
            this.viewModel.Context = new TestDotvvmRequestContext {
                Configuration = DotvvmTestHelper.CreateConfiguration()
            };

            this.viewModel.Context.Services.GetRequiredService<IViewModelSerializationMapper>().Map(typeof(ViewModel), m => {
                m.Property(nameof(ViewModel.RenamedProperty)).Name = "rp";
            });
        }

        [TestMethod]
        public void ModelState_SinglePropertyExpression()
        {
            Assert.AreEqual("/MyProperty", ValidationErrorFactory.CreateModelError(viewModel, v => v.MyProperty, "").PropertyPath);
        }

        [TestMethod]
        public void ModelState_SingleRenamedProperty()
        {
            Assert.AreEqual("/rp", ValidationErrorFactory.CreateModelError(viewModel, v => v.RenamedProperty, "").PropertyPath);
        }

        [TestMethod]
        public void ModelState_NestedViewModelExpression()
        {
            Assert.AreEqual("/AnotherProperty/StringProp", ValidationErrorFactory.CreateModelError(viewModel, v => v.AnotherProperty.StringProp, "").PropertyPath);
        }

        [TestMethod]
        public void ModelState_ArrayAccessExpression()
        {
            Assert.AreEqual("/AnotherProperty/TestViewModel2/Collection[0]/StringValue", ValidationErrorFactory.CreateModelError(viewModel, v => v.AnotherProperty.TestViewModel2.Collection[0].StringValue, "").PropertyPath);
        }

        [TestMethod]
        public void ModelState_ValidatableObjectHelpers()
        {
            var result = viewModel.Validate(null).ToArray();
            var memberNames = result[0].MemberNames.ToArray();
            Assert.AreEqual(memberNames[0], "/AnotherProperty/EnumProperty");
            Assert.AreEqual(memberNames[1], "/rp");
        }

        class ViewModel: DotvvmViewModelBase, IValidatableObject
        {
            public int MyProperty { get; set; }
            public int RenamedProperty { get; set; }
            public TestViewModel AnotherProperty { get; set; }
            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                yield return ValidationErrorFactory.CreateValidationResult(this, "Error", t => t.AnotherProperty.EnumProperty, t => t.RenamedProperty);
            }
        }
    }
}
