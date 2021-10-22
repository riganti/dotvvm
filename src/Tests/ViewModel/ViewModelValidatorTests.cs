using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Validation;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class ViewModelValidatorTests
    {
        private IViewModelValidator CreateValidator() => DotvvmTestHelper.CreateConfiguration().ServiceProvider.GetRequiredService<IViewModelValidator>();
        private IValidationErrorPathExpander CreateErrorPathExpander() => DotvvmTestHelper.CreateConfiguration().ServiceProvider.GetRequiredService<IValidationErrorPathExpander>();

        [TestMethod]
        public void ViewModelValidator_SimpleObject()
        {
            var testViewModel = new TestViewModel();
            var validator = CreateValidator();
            var expander = CreateErrorPathExpander();
            var modelState = new ModelState() { ValidationTarget = testViewModel };

            var errors = validator.ValidateViewModel(testViewModel).OrderBy(n => n.PropertyPath);
            modelState.ErrorsInternal.AddRange(errors);
            expander.Expand(modelState, testViewModel);
            var results = modelState.Errors.OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("/Text", results[0].PropertyPath);
        }

        [TestMethod]
        public void ViewModelValidator_ObjectWithCollection()
        {
            var testViewModel = new TestViewModel()
            {
                Children = new List<TestViewModel2>()
                {
                    new TestViewModel2() { Code = "012" },
                    new TestViewModel2() { Code = "ABC", Id = 13 },
                    new TestViewModel2() { Code = "345", Id = 15 }
                },
                Child = new TestViewModel2() {  Code = "123" }
            };
            var validator = CreateValidator();
            var expander = CreateErrorPathExpander();
            var modelState = new ModelState() { ValidationTarget = testViewModel };

            var errors = validator.ValidateViewModel(testViewModel);
            modelState.ErrorsInternal.AddRange(errors);
            expander.Expand(modelState, testViewModel);
            var results = modelState.Errors.OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("/Child/Id", results[0].PropertyPath);
            Assert.AreEqual("/Children/0/Id", results[1].PropertyPath);
            Assert.AreEqual("/Children/1/Code", results[2].PropertyPath);
            Assert.AreEqual("/Text", results[3].PropertyPath);
        }

        [TestMethod]
        public void ViewModelValidator_ServerOnlyRules()
        {
            var testViewModel = new TestViewModel3() { Email = "aaa" };
            var validator = CreateValidator();
            var expander = CreateErrorPathExpander();
            var modelState = new ModelState() { ValidationTarget = testViewModel };

            var errors = validator.ValidateViewModel(testViewModel).OrderBy(n => n.PropertyPath);
            modelState.ErrorsInternal.AddRange(errors);
            expander.Expand(modelState, testViewModel);
            var results = modelState.Errors.OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("/Email", results[0].PropertyPath);
        }

        [TestMethod]
        public void ViewModelValidator_WithValidationTarget_Property()
        {
            var testViewModel = new TestViewModel() { Child = new TestViewModel2() { Code = "5" } };
            var validator = CreateValidator();
            var expander = CreateErrorPathExpander();
            var modelState = new ModelState() { ValidationTarget = testViewModel.Child };

            var errors = validator.ValidateViewModel(testViewModel.Child).OrderBy(n => n.PropertyPath);
            modelState.ErrorsInternal.AddRange(errors);
            expander.Expand(modelState, testViewModel);
            var results = modelState.Errors.OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("/Child/Code", results[0].PropertyPath);
            Assert.AreEqual("/Child/Id", results[1].PropertyPath);
        }

        [TestMethod]
        public void ViewModelValidator_WithValidationTarget_ArrayElement()
        {
            var testViewModel = new TestViewModel()
            {
                Children = new List<TestViewModel2>()
                {
                    new TestViewModel2() { Code = "5" },
                    new TestViewModel2() { Code = "6" },
                    new TestViewModel2() { Code = "7" },
                }
            };
            var validator = CreateValidator();
            var expander = CreateErrorPathExpander();
            var modelState = new ModelState() { ValidationTarget = testViewModel.Children[1] };

            var errors = validator.ValidateViewModel(testViewModel.Children[1]).OrderBy(n => n.PropertyPath);
            modelState.ErrorsInternal.AddRange(errors);
            expander.Expand(modelState, testViewModel);
            var results = modelState.Errors.OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("/Children/1/Code", results[0].PropertyPath);
            Assert.AreEqual("/Children/1/Id", results[1].PropertyPath);
        }

        [TestMethod]
        public void ViewModelValidator_Child_CustomValidationAttribute()
        {
            var testViewModel = new TestViewModel4() { Child = new TestViewModel4Child() { IsChecked = true } };
            var validator = CreateValidator();
            var expander = CreateErrorPathExpander();
            var modelState = new ModelState() { ValidationTarget = testViewModel };

            var errors = validator.ValidateViewModel(testViewModel).OrderBy(n => n.PropertyPath);
            modelState.ErrorsInternal.AddRange(errors);
            expander.Expand(modelState, testViewModel);
            var results = modelState.Errors.OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("/Child/ConditionalRequired", results[0].PropertyPath);
        }

        [TestMethod]
        public void ViewModelValidator_ListChild_CustomValidationAttribute()
        {
            var testViewModel = new List<TestViewModel4>{
                new TestViewModel4() { Child = new TestViewModel4Child() { IsChecked = true } }
            };
            var validator = CreateValidator();
            var expander = CreateErrorPathExpander();
            var modelState = new ModelState() { ValidationTarget = testViewModel };

            var errors = validator.ValidateViewModel(testViewModel).OrderBy(n => n.PropertyPath);
            modelState.ErrorsInternal.AddRange(errors);
            expander.Expand(modelState, testViewModel);
            var results = modelState.Errors.OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("/0/Child/ConditionalRequired", results[0].PropertyPath);
        }

        [TestMethod]
        public void ViewModelValidator_Child_IValidatableObject()
        {
            var testViewModel = new TestViewModel5() { Child = new TestViewModel5Child() { IsChecked = true } };
            var validator = CreateValidator();
            var expander = CreateErrorPathExpander();
            var modelState = new ModelState() { ValidationTarget = testViewModel };

            var errors = validator.ValidateViewModel(testViewModel).OrderBy(n => n.PropertyPath);
            modelState.ErrorsInternal.AddRange(errors);
            expander.Expand(modelState, testViewModel);
            var results = modelState.Errors.OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("/Child/ConditionalRequired", results[0].PropertyPath);
        }

        [TestMethod]
        public void ViewModelValidator_CollectionOfIValidatableObjects()
        {
            var testViewModel = new TestViewModel6()
            {
                Children = new List<TestViewModel5Child>() {new TestViewModel5Child() {IsChecked = true}}
            };
            var validator = CreateValidator();
            var expander = CreateErrorPathExpander();
            var modelState = new ModelState() { ValidationTarget = testViewModel };

            var errors = validator.ValidateViewModel(testViewModel).OrderBy(n => n.PropertyPath);
            modelState.ErrorsInternal.AddRange(errors);
            expander.Expand(modelState, testViewModel);
            var results = modelState.Errors.OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("/Children/0/ConditionalRequired", results[0].PropertyPath);
        }

        [TestMethod]
        public void ViewModelValidator_ValidationErrorFactoryWithValidationContextSupplied()
        {
            var testViewModel = new TestViewModel7();
            var validator = CreateValidator();
            var expander = CreateErrorPathExpander();
            var modelState = new ModelState() { ValidationTarget = testViewModel };

            var errors = validator.ValidateViewModel(testViewModel).OrderBy(n => n.PropertyPath);
            modelState.ErrorsInternal.AddRange(errors);
            expander.Expand(modelState, testViewModel);
            var results = modelState.Errors.OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("/IsChecked", results[0].PropertyPath);
        }

        [TestMethod]
        public void ViewModelValidator_CustomModelStateErrors()
        {
            var testViewModel = new TestViewModel()
            {
                Context = new DotvvmRequestContext(null, DotvvmTestHelper.CreateConfiguration(), null),
                Child = new TestViewModel2()
                {
                    Id = 11,
                    Code = "Code",
                },
            };
            var validator = CreateValidator();
            var expander = CreateErrorPathExpander();
            var modelState = testViewModel.Context.ModelState;
            var validationTarget = testViewModel;
            modelState.ValidationTarget = validationTarget;

            ValidationErrorFactory.AddModelError(testViewModel, vm => vm, "Custom root error.");
            var errors = validator.ValidateViewModel(validationTarget).OrderBy(n => n.PropertyPath);
            modelState.ErrorsInternal.AddRange(errors);
            expander.Expand(modelState, testViewModel);
            var results = modelState.Errors.OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("/", results[0].PropertyPath);
            Assert.AreEqual("/Child/Code", results[1].PropertyPath);
            Assert.AreEqual("/Text", results[2].PropertyPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ViewModelValidator_CustomModelStateErrors_OldFormatThrows()
        {
            var testViewModel = new TestViewModel() {
                Context = new DotvvmRequestContext(null, DotvvmTestHelper.CreateConfiguration(), null),
                Child = new TestViewModel2() {
                    Id = 11,
                    Code = "Code",
                },
            };
            var validator = CreateValidator();
            var expander = CreateErrorPathExpander();
            var modelState = testViewModel.Context.ModelState;
            var validationTarget = testViewModel;
            modelState.ValidationTarget = validationTarget;

            testViewModel.AddModelError("Child()", "Validation target path as a knockout expression");
            var errors = validator.ValidateViewModel(validationTarget).OrderBy(n => n.PropertyPath);
            modelState.ErrorsInternal.AddRange(errors);
            expander.Expand(modelState, testViewModel);
        }

        [TestMethod]
        public void ViewModelValidator_CustomModelStateErrors_OutsideValidationTarget()
        {
            var testViewModel = new TestViewModel()
            {
                Context = new DotvvmRequestContext(null, DotvvmTestHelper.CreateConfiguration(), null),
                Child = new TestViewModel2()
                {
                    Id = 11,
                    Code = "Code",
                },
                Children = new List<TestViewModel2>()
                {
                    new TestViewModel2() { Code = "5" },
                    new TestViewModel2() { Code = "6" },
                    new TestViewModel2() { Code = "7" },
                }
            };
            var validator = CreateValidator();
            var expander = CreateErrorPathExpander();
            var modelState = testViewModel.Context.ModelState;
            var validationTarget = testViewModel.Children[0];
            modelState.ValidationTarget = validationTarget;

            testViewModel.AddModelError(vm => vm, "Custom root error. Outside of validation target.");
            testViewModel.AddModelError(vm => vm.Child, "Custom Child error. Outside of validation target.");
            testViewModel.AddModelError(vm => vm.Children[2], "Custom Children[2] error. Outside of validation target.");

            var errors = validator.ValidateViewModel(validationTarget).OrderBy(n => n.PropertyPath);
            modelState.ErrorsInternal.AddRange(errors);
            expander.Expand(modelState, testViewModel);
            var results = modelState.Errors.OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual("/", results[0].PropertyPath);
            Assert.AreEqual("/Child", results[1].PropertyPath);
            Assert.AreEqual("/Children/0/Code", results[2].PropertyPath);
            Assert.AreEqual("/Children/0/Id", results[3].PropertyPath);
            Assert.AreEqual("/Children/2", results[4].PropertyPath);
        }

        [TestMethod]
        public void ViewModelValidator_CustomModelStateErrors_ArbitraryTargetObjectAndLambda()
        {
            var testViewModel = new TestViewModel() {
                Context = new DotvvmRequestContext(null, DotvvmTestHelper.CreateConfiguration(), null),
                Child = new TestViewModel2() {
                    Id = 11,
                    Code = "Code",
                },
                Children = new List<TestViewModel2>()
                {
                    new TestViewModel2() { Code = "5" },
                    new TestViewModel2() { Code = "6" },
                    new TestViewModel2() { Code = "7" },
                }
            };
            var context = testViewModel.Context;
            var validator = CreateValidator();
            var expander = CreateErrorPathExpander();
            var modelState = context.ModelState;
            var validationTarget = testViewModel.Children[1];
            modelState.ValidationTarget = validationTarget;

            context.AddModelError(testViewModel.Children[1], o => o.Code, "Custom /Children/1/Code error.");

            // Add error that is unreachable from root viewmodel
            context.AddModelError(new TestViewModel2(), o => o.Id, "Unreachable error - won't be resolved.");

            var errors = validator.ValidateViewModel(validationTarget).OrderBy(n => n.PropertyPath);
            modelState.ErrorsInternal.AddRange(errors);
            expander.Expand(modelState, testViewModel);
            var results = modelState.Errors.OrderBy(n => n.PropertyPath).ToList();

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("/Children/1/Code", results[0].PropertyPath);
            Assert.AreEqual("/Children/1/Code", results[1].PropertyPath);
            Assert.AreEqual("/Children/1/Id", results[2].PropertyPath);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ViewModelValidator_ObjectWithAttachedErrorReferencedMultipleTimes()
        {
            var innerViewModel = new TestViewModel2() { Code = "123" };
            var testViewModel = new TestViewModel()
            {
                Context = new DotvvmRequestContext(null, DotvvmTestHelper.CreateConfiguration(), null),
                Child = new TestViewModel2()
                {
                    Id = 11,
                    Code = "Code",
                },
                Children = new List<TestViewModel2>()
                {
                    innerViewModel,
                    new TestViewModel2() { Code = "6" },
                    innerViewModel,
                }
            };
            var validator = CreateValidator();
            var expander = CreateErrorPathExpander();
            var modelState = testViewModel.Context.ModelState;
            var validationTarget = testViewModel;
            modelState.ValidationTarget = validationTarget;

            testViewModel.AddModelError(vm => vm.Children[0], "An error on object that is found multiple times in viewmodel.");

            var errors = validator.ValidateViewModel(validationTarget).OrderBy(n => n.PropertyPath);
            modelState.ErrorsInternal.AddRange(errors);
            expander.Expand(modelState, testViewModel);
        }


        public class TestViewModel : DotvvmViewModelBase
        {
            [Required]
            public string Text { get; set; }

            public List<TestViewModel2> Children { get; set; }

            public TestViewModel2 Child { get; set; }
        }

        public class TestViewModel2
        {
            [Required]
            public int? Id { get; set; }

            [RegularExpression("^[0-9]{3}$")]
            public string Code { get; set; }
        }

        public class TestViewModel3
        {
            [EmailAddress]
            public string Email { get; set; }
        }

        public class TestViewModel4
        {

            public TestViewModel4Child Child { get; set; }

        }

        public class TestViewModel4Child
        {

            public bool IsChecked { get; set; }

            [ConditionalRequiredValue]
            public string ConditionalRequired { get; set; }

        }

        public class ConditionalRequiredValueAttribute : ValidationAttribute
        {
            public override bool RequiresValidationContext => true;

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var entity = (TestViewModel4Child)validationContext.ObjectInstance;
                if (entity.IsChecked && string.IsNullOrEmpty(entity.ConditionalRequired))
                {
                    return new ValidationResult("Value is required when the field is checked!", new[] { validationContext.MemberName });    
                }

                return base.IsValid(value, validationContext);
            }
        }

        public class TestViewModel5
        {
            public TestViewModel5Child Child { get; set; }
        }

        public class TestViewModel5Child : IValidatableObject
        {
            public bool IsChecked { get; set; }

            public string ConditionalRequired { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (IsChecked && string.IsNullOrEmpty(ConditionalRequired))
                {
                    yield return new ValidationResult("Value is required when the field is checked!", new[] { nameof(ConditionalRequired) });
                }
            }
        }

        public class TestViewModel6
        {
            public List<TestViewModel5Child> Children { get; set; }
        }

        public class TestViewModel7 : IValidatableObject
        {
            public bool IsChecked { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (!IsChecked)
                {
                    yield return ValidationErrorFactory.CreateValidationResult<TestViewModel7>(validationContext,
                        "Value is required when the field is checked!", vm => vm.IsChecked);
                }
            }
        }
    }

}
