#nullable enable

using System;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Binding
{
    [TestClass]
    public class BindingCombinatorTests
    {
        private BindingTestHelper bindings = new BindingTestHelper();
        private DataContextStack context;

        public BindingCombinatorTests()
        {
            context = bindings.CreateDataContext([typeof(BooleanViewModel)]);
        }

        [TestMethod]
        public void GetCombination_ComputesOnceAndCachesByDescriptorAndOrderedBindings()
        {
            var a = new TestBinding();
            var b = new TestBinding();
            var calls = 0;
            var descriptor = new BindingCombinator.BindingCombinatorDescriptor((left, right) => {
                calls++;
                return left;
            });

            var first = descriptor.GetCombination(a, b);
            var same = descriptor.GetCombination(a, b);
            var reversed = descriptor.GetCombination(b, a);
            var otherDescriptor = new BindingCombinator.BindingCombinatorDescriptor((left, right) => right);

            Assert.AreSame(a, first);
            Assert.AreSame(first, same);
            Assert.AreSame(b, reversed);
            Assert.AreEqual(2, calls);
            Assert.AreSame(b, otherDescriptor.GetCombination(a, b));
        }

        [TestMethod]
        public void GetCombination_CachesFactoryException()
        {
            var a = new TestBinding();
            var b = new TestBinding();
            var calls = 0;
            var descriptor = new BindingCombinator.BindingCombinatorDescriptor((_, _) => {
                calls++;
                throw new InvalidOperationException("Expected failure");
            });

            Assert.ThrowsException<InvalidOperationException>(() => descriptor.GetCombination(a, b));
            Assert.ThrowsException<InvalidOperationException>(() => descriptor.GetCombination(a, b));
            Assert.AreEqual(1, calls);
        }

        [TestMethod]
        [DataRow(false, false, false, false)]
        [DataRow(false, true, false, true)]
        [DataRow(true, false, false, true)]
        [DataRow(true, true, true, true)]
        public void BooleanDescriptors_CreateEvaluatableShortCircuitExpressions(bool leftValue, bool rightValue, bool expectedAnd, bool expectedOr)
        {
            var left = BoolBinding(nameof(BooleanViewModel.Left));
            var right = BoolBinding(nameof(BooleanViewModel.Right));
            var and = (IStaticValueBinding<bool>)BindingCombinator.AndAlsoCombination.GetCombination(left, right);
            var or = (IStaticValueBinding<bool>)BindingCombinator.OrElseCombination.GetCombination(left, right);
            var control = CreateControl(new BooleanViewModel { Left = leftValue, Right = rightValue });

            Assert.AreEqual(expectedAnd, and.Evaluate(control));
            Assert.AreEqual(expectedOr, or.Evaluate(control));
            Assert.AreSame(and, BindingCombinator.AndAlsoCombination.GetCombination(left, right));
            Assert.AreSame(or, BindingCombinator.OrElseCombination.GetCombination(left, right));
        }

        [TestMethod]
        public void AndAssignProperty_RejectsUnsupportedPropertyAndOperands()
        {
            var control = new HtmlGenericControl("div");

            var exception = Assert.ThrowsException<NotSupportedException>(() =>
                control.AndAssignProperty(HtmlGenericControl.InnerTextProperty, "text"));

            StringAssert.Contains(exception.Message, "Can only AND boolean properties");
        }

        [TestMethod]
        public void AndAssignProperty_AppliesBooleanIdentitiesWithoutCreatingBindings()
        {
            var binding = BoolBinding(nameof(BooleanViewModel.Left));

            var unset = new HtmlGenericControl("div");
            unset.AndAssignProperty(HtmlGenericControl.VisibleProperty, false);
            Assert.AreEqual(false, unset.GetValueRaw(HtmlGenericControl.VisibleProperty));
            AssertAssignment(false, binding, false);
            AssertAssignment(binding, true, binding);
            AssertAssignment(true, binding, binding);
            AssertAssignment(binding, false, false);
        }

        [TestMethod]
        public void AndAssignProperty_CombinesTwoValueBindingsAndReusesCombination()
        {
            var left = BoolBinding(nameof(BooleanViewModel.Left));
            var right = BoolBinding(nameof(BooleanViewModel.Right));
            var control = CreateControl(new BooleanViewModel { Left = true, Right = false });

            control.SetValue(HtmlGenericControl.VisibleProperty, left);
            control.AndAssignProperty(HtmlGenericControl.VisibleProperty, right);
            var combined = (IStaticValueBinding<bool>)control.GetValueRaw(HtmlGenericControl.VisibleProperty)!;

            Assert.IsFalse(combined.Evaluate(control));
            Assert.AreSame(
                BindingCombinator.AndAlsoCombination.GetCombination(left, right),
                combined);
        }

        [TestMethod]
        public void AndAssignProperty_EvaluatesResourceBindingWhenMixedWithValueBinding()
        {
            var valueBinding = BoolBinding(nameof(BooleanViewModel.Left));
            var resourceBinding = bindings.ResourceBinding<bool>(nameof(BooleanViewModel.Right), context);
            var control = CreateControl(new BooleanViewModel { Left = true, Right = true });

            control.SetValue(HtmlGenericControl.VisibleProperty, resourceBinding);
            control.AndAssignProperty(HtmlGenericControl.VisibleProperty, valueBinding);
            Assert.AreSame(valueBinding, control.GetValueRaw(HtmlGenericControl.VisibleProperty));

            control.DataContext = new BooleanViewModel { Left = true, Right = false };
            control.SetValue(HtmlGenericControl.VisibleProperty, valueBinding);
            control.AndAssignProperty(HtmlGenericControl.VisibleProperty, resourceBinding);
            Assert.AreEqual(false, control.GetValueRaw(HtmlGenericControl.VisibleProperty));
        }


        private ValueBindingExpression<bool> BoolBinding(string property) => bindings.ValueBinding<bool>(property, context);

        private void AssertAssignment(object current, object assigned, object expected)
        {
            var control = CreateControl(new BooleanViewModel { Left = true });
            control.SetValue(HtmlGenericControl.VisibleProperty, current);
            control.AndAssignProperty(HtmlGenericControl.VisibleProperty, assigned);
            var actual = control.GetValueRaw(HtmlGenericControl.VisibleProperty);
            if (expected is IBinding)
                Assert.AreSame(expected, actual);
            else
                Assert.AreEqual(expected, actual);
        }

        private HtmlGenericControl CreateControl(BooleanViewModel viewModel)
        {
            var control = new HtmlGenericControl("div") { DataContext = viewModel };
            control.SetDataContextType(context);
            return control;
        }

        private sealed class BooleanViewModel
        {
            public bool Left { get; set; }
            public bool Right { get; set; }
        }

        private sealed class TestBinding : IBinding
        {
            public DataContextStack? DataContext => null;
            public BindingResolverCollection? GetAdditionalResolvers() => null;
            public object? GetProperty(Type type, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException) => null;
        }
    }
}
