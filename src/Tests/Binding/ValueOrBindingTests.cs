#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Binding
{
    [TestClass]
    public class ValueOrBindingTests
    {
        private BindingTestHelper bindings = null!;
        private DataContextStack context = null!;

        [TestInitialize]
        public void Initialize()
        {
            bindings = new BindingTestHelper();
            context = bindings.CreateDataContext(new[] { typeof(TestViewModel) });
        }

        [TestMethod]
        public void Constructors_PreserveStateAndValidateBindings()
        {
            ValueOrBinding<int> defaultNumber = default;
            var nullString = new ValueOrBinding<string?>((string?)null);
            var numberBinding = Binding<int>(nameof(TestViewModel.Number));
            var boundNumber = new ValueOrBinding<int>(numberBinding);
            var exception = Assert.ThrowsException<BindingHelper.InvalidBindingTypeException>(() =>
                new ValueOrBinding<string>((IBinding)numberBinding));

            AssertValue(0, defaultNumber);
            AssertValue<string?>(null, nullString);
            AssertBinding(numberBinding, boundNumber);
            Assert.AreEqual("null", nullString.ToString());
            Assert.AreSame(numberBinding, exception.Binding);
            Assert.ThrowsException<ArgumentNullException>(() => new ValueOrBinding<int>((IBinding)null!));
            Assert.ThrowsException<ArgumentNullException>(() => new ValueOrBinding<int>((IStaticValueBinding<int>)null!));
        }

        [TestMethod]
        public void FromBoxedValue_HandlesRawValuesBindingsAndValueContainers()
        {
            var binding = Binding<int>(nameof(TestViewModel.Number));

            var raw = ValueOrBinding<int>.FromBoxedValue(12);
            var directBinding = ValueOrBinding<int>.FromBoxedValue(binding);
            var nestedValue = ValueOrBinding<int>.FromBoxedValue(new ValueOrBinding<int>(13));

            AssertValue(12, raw);
            AssertBinding(binding, directBinding);
            AssertValue(13, nestedValue);
        }

        [TestMethod]
        public void Evaluate_ReturnsValueOrEvaluatesBinding()
        {
            var control = CreateControl(new TestViewModel { Number = 42 });
            var binding = Binding<int>(nameof(TestViewModel.Number));

            Assert.AreEqual(7, new ValueOrBinding<int>(7).Evaluate(control));
            Assert.AreEqual(42, new ValueOrBinding<int>(binding).Evaluate(control));
            Assert.AreEqual("12", new ValueOrBinding<int>(12).ToString());
            Assert.AreEqual(binding.ToString(), new ValueOrBinding<int>(binding).ToString());
        }

        [TestMethod]
        public void GetValueAndGetBinding_RejectWrongState()
        {
            var binding = Binding<int>(nameof(TestViewModel.Number));
            var withValue = new ValueOrBinding<int>(7);
            var withBinding = new ValueOrBinding<int>(binding);

            var bindingExpected = Assert.ThrowsException<DotvvmControlException>(() => withValue.GetBinding());
            var valueExpected = Assert.ThrowsException<DotvvmControlException>(() => withBinding.GetValue());

            StringAssert.Contains(bindingExpected.Message, "contains a value: 7");
            StringAssert.Contains(valueExpected.Message, "contains a binding");
            Assert.AreSame(binding, valueExpected.RelatedBinding);
        }

        [TestMethod]
        public void CastHelpers_PreserveValuesAndBindings()
        {
            var stringBinding = Binding<string>(nameof(TestViewModel.Text));
            var downcastValue = ValueOrBinding<object>.DownCast(new ValueOrBinding<string>("text"));
            var downcastBinding = ValueOrBinding<object>.DownCast(new ValueOrBinding<string>(stringBinding));
            var upcastValue = new ValueOrBinding<object>("text").UpCast<string>();
            var upcastBinding = new ValueOrBinding<object>((IBinding)stringBinding).UpCast<string>();
            ValueOrBinding nonGenericValue = new ValueOrBinding<string>("text");
            ValueOrBinding nonGenericBinding = new ValueOrBinding<string>(stringBinding);

            AssertValue<object>("text", downcastValue);
            AssertBinding(stringBinding, downcastBinding);
            AssertValue("text", upcastValue);
            AssertBinding(stringBinding, upcastBinding);
            AssertValue("text", ValueOrBinding<string>.UpCast(nonGenericValue));
            AssertBinding(stringBinding, ValueOrBinding<string>.UpCast(nonGenericBinding));
            Assert.ThrowsException<InvalidCastException>(() => new ValueOrBinding<object>(1).UpCast<string>());
        }

        [TestMethod]
        public void Process_InvokesOnlyBranchMatchingState()
        {
            var binding = Binding<int>(nameof(TestViewModel.Number));
            var valueCalls = 0;
            var bindingCalls = 0;

            new ValueOrBinding<int>(5).Process(_ => valueCalls++, _ => bindingCalls++);
            new ValueOrBinding<int>(binding).Process(_ => valueCalls++, _ => bindingCalls++);
            var valueResult = new ValueOrBinding<int>(5).Process(v => v * 2, _ => -1);
            var bindingResult = new ValueOrBinding<int>(binding).Process(_ => -1, b => ReferenceEquals(b, binding) ? 1 : 0);

            Assert.AreEqual(1, valueCalls);
            Assert.AreEqual(1, bindingCalls);
            Assert.AreEqual(10, valueResult);
            Assert.AreEqual(1, bindingResult);
        }

        [TestMethod]
        public void ProcessValueBinding_TreatsValuesAndResourceBindingsAsValues()
        {
            var control = CreateControl(new TestViewModel { Number = 42 });
            var resource = ResourceBinding<int>(nameof(TestViewModel.Number));
            var valueBinding = Binding<int>(nameof(TestViewModel.Number));

            var rawResult = new ValueOrBinding<int>(5).ProcessValueBinding(control, v => $"value:{v}", _ => "binding");
            var resourceResult = new ValueOrBinding<int>(resource).ProcessValueBinding(control, v => $"value:{v}", _ => "binding");
            var bindingResult = new ValueOrBinding<int>(valueBinding).ProcessValueBinding(control, _ => "value", b => ReferenceEquals(b, valueBinding) ? "binding" : "other");
            string? actionResult = null;
            new ValueOrBinding<int>(resource).ProcessValueBinding(control, v => actionResult = $"value:{v}", _ => actionResult = "binding");

            Assert.AreEqual("value:5", rawResult);
            Assert.AreEqual("value:42", resourceResult);
            Assert.AreEqual("binding", bindingResult);
            Assert.AreEqual("value:42", actionResult);
        }

        [TestMethod]
        public void EvaluateResourceBinding_OnlyUnwrapsStaticNonValueBinding()
        {
            var control = CreateControl(new TestViewModel { Number = 42 });
            var resource = new ValueOrBinding<int>(ResourceBinding<int>(nameof(TestViewModel.Number)));
            var valueBinding = Bound<int>(nameof(TestViewModel.Number));
            var value = new ValueOrBinding<int>(5);

            var evaluatedResource = resource.EvaluateResourceBinding(control);
            var untouchedValueBinding = valueBinding.EvaluateResourceBinding(control);
            var untouchedValue = value.EvaluateResourceBinding(control);

            AssertValue(42, evaluatedResource);
            AssertBinding(valueBinding.BindingOrDefault!, untouchedValueBinding);
            AssertValue(5, untouchedValue);
        }

        [TestMethod]
        public void JavascriptExpressions_SerializeValuesAndUseValueBindings()
        {
            var control = CreateControl(new TestViewModel { Text = "hello" });
            var value = new ValueOrBinding<string>("hello");
            var binding = Bound<string>(nameof(TestViewModel.Text));
            var ordinary = new object();

            Assert.AreEqual("\"hello\"", value.GetJsExpression(control));
            Assert.AreEqual("\"hello\"", value.GetParametrizedJsExpression(control).ToDefaultString());
            StringAssert.Contains(binding.GetJsExpression(control), nameof(TestViewModel.Text));
            StringAssert.Contains(binding.GetJsExpression(control, unwrapped: true), nameof(TestViewModel.Text));
            Assert.AreEqual("hello", value.UnwrapToObject());
            Assert.AreSame(binding.BindingOrDefault, binding.UnwrapToObject());
            Assert.AreSame(ordinary, ValueOrBindingExtensions.UnwrapToObject(ordinary));
            Assert.IsNull(ValueOrBindingExtensions.UnwrapToObject((object?)null));
        }

        [TestMethod]
        public void UnwrapToObject_UntypedNullReturnsNull()
        {
            Assert.IsNull(ValueOrBindingExtensions.UnwrapToObject(null));
        }

        [TestMethod]
        public void FromBoxedValue_ValueContainerWithBindingPreservesBinding()
        {
            var binding = Binding<int>(nameof(TestViewModel.Number));
            ValueOrBinding boxed = new ValueOrBinding<int>(binding);

            var result = ValueOrBinding<int>.FromBoxedValue(boxed);

            Assert.AreSame(binding, result.GetBinding());
        }


        [TestMethod]
        public void ValueAndNullPredicates_DistinguishValuesFromBindings()
        {
            var binding = Bound<string?>(nameof(TestViewModel.NullableText));

            Assert.IsTrue(new ValueOrBinding<int>(5).ValueEquals(5));
            Assert.IsFalse(new ValueOrBinding<int>(5).ValueEquals(6));
            Assert.IsTrue(new ValueOrBinding<string>("TEST").ValueEquals("test", StringComparer.OrdinalIgnoreCase));
            Assert.IsFalse(binding.ValueEquals(null));
            Assert.IsTrue(new ValueOrBinding<string?>((string?)null).ValueIsNull());
            Assert.IsFalse(binding.ValueIsNull());
            Assert.IsTrue(new ValueOrBinding<string?>((string?)null).ValueIsNullOrEmpty());
            Assert.IsTrue(new ValueOrBinding<string?>("").ValueIsNullOrEmpty());
            Assert.IsFalse(binding.ValueIsNullOrEmpty());
            Assert.IsTrue(new ValueOrBinding<string?>((string?)null).IsNull().GetValue());
            Assert.IsFalse(new ValueOrBinding<string>("text").IsNull().GetValue());
            Assert.IsTrue(new ValueOrBinding<string>("text").NotNull().GetValue());
            Assert.IsTrue(new ValueOrBinding<string>((string)null!).IsNullOrEmpty().GetValue());
            Assert.IsTrue(new ValueOrBinding<string>("").IsNullOrEmpty().GetValue());
            Assert.IsTrue(new ValueOrBinding<string>(" ").IsNullOrWhitespace().GetValue());
            Assert.IsTrue(new ValueOrBinding<string>("text").NotNullOrEmpty().GetValue());
            Assert.IsTrue(new ValueOrBinding<string>("text").NotNullOrWhitespace().GetValue());
        }

        [TestMethod]
        public void SimpleTransformations_ComputeConstantValues()
        {
            var items = new List<int> { 1, 2 };
            var dataSet = new GridViewDataSet<int> { Items = items };

            Assert.IsFalse(new ValueOrBinding<bool>(true).Negate().GetValue());
            Assert.IsNull(new ValueOrBinding<bool?>((bool?)null).Negate().GetValue());
            Assert.IsTrue(new ValueOrBinding<int>(1).IsMoreThanZero().GetValue());
            Assert.IsFalse(new ValueOrBinding<int>(0).IsMoreThanZero().GetValue());
            Assert.AreSame(items, new ValueOrBinding<IGridViewDataSet<int>>(dataSet).GetItems().GetValue());
            Assert.AreEqual("", new ValueOrBinding<object?>((object?)null).AsString().GetValue());
            Assert.AreEqual("12", new ValueOrBinding<int>(12).AsString().GetValue());
            Assert.AreEqual("FirstValue", new ValueOrBinding<TestEnum>(TestEnum.FirstValue).AsString().GetValue());
        }

        [TestMethod]
        public void BindingTransformations_ReturnCachedEvaluatableBindings()
        {
            var control = CreateControl(new TestViewModel());
            var flagBinding = Binding<bool>(nameof(TestViewModel.Flag));
            var flag = new ValueOrBinding<bool>(flagBinding);
            var nullableFlag = Bound<bool?>(nameof(TestViewModel.NullableFlag));
            var number = Bound<int>(nameof(TestViewModel.Number));
            var text = Bound<string>(nameof(TestViewModel.Text));
            var nullableText = Bound<string?>(nameof(TestViewModel.NullableText));
            var dataSet = new ValueOrBinding<IGridViewDataSet<int>>(Binding<GridViewDataSet<int>>(nameof(TestViewModel.DataSet)));

            Assert.IsFalse(flag.Negate().Evaluate(control));
            Assert.IsFalse(flagBinding.Negate().Evaluate(control));
            Assert.IsNull(nullableFlag.Negate().Evaluate(control));
            Assert.IsTrue(number.IsMoreThanZero().Evaluate(control));
            Assert.AreEqual("2", number.AsString().Evaluate(control));
            Assert.IsFalse(text.IsNullOrEmpty().Evaluate(control));
            Assert.IsFalse(text.IsNullOrWhitespace().Evaluate(control));
            Assert.IsTrue(text.NotNullOrEmpty().Evaluate(control));
            Assert.IsTrue(text.NotNullOrWhitespace().Evaluate(control));
            Assert.IsTrue(nullableText.IsNull().Evaluate(control));
            CollectionAssert.AreEqual(new[] { 1, 2 }, dataSet.GetItems().Evaluate(control)!.ToArray());
            Assert.AreSame(flag.Negate().BindingOrDefault, flag.Negate().BindingOrDefault);
            Assert.AreSame(text.IsNullOrEmpty().BindingOrDefault, text.IsNullOrEmpty().BindingOrDefault);
        }

        [TestMethod]
        public void GetItems_InterfaceTypedBindingReturnsItems()
        {
            var expected = new List<int> { 1, 2 };
            var control = CreateControl(new TestViewModel { InterfaceDataSet = new GridViewDataSet<int> { Items = expected } });
            var binding = Binding<IGridViewDataSet<int>>(nameof(TestViewModel.InterfaceDataSet));
            var dataSet = new ValueOrBinding<IGridViewDataSet<int>>(binding);

            Assert.AreSame(expected, dataSet.GetItems().Evaluate(control));
        }

        [TestMethod]
        public void GetItems_InterfaceTypedBindingFails()
        {
            var binding = Binding<IPageableGridViewDataSet<PagingOptions>>(nameof(TestViewModel.InterfacePageableDataSet));
            // binding.GetProperty<DataSourceAccessBinding>();
            var exception = XAssert.ThrowsAny<Exception>(() => binding.GetProperty<DataSourceAccessBinding>());
            StringAssert.Contains(exception.Message, "untyped IGridViewDataSet");
        }

        [TestMethod]
        public void AndOr_ApplyBooleanIdentitiesAndCombineBindings()
        {
            var leftBinding = Binding<bool>(nameof(TestViewModel.Flag));
            var rightBinding = Binding<bool>(nameof(TestViewModel.OtherFlag));
            var left = new ValueOrBinding<bool>(leftBinding);
            var right = new ValueOrBinding<bool>(rightBinding);
            var control = CreateControl(new TestViewModel());

            AssertValue(false, new ValueOrBinding<bool>(false).And(left));
            AssertBinding(leftBinding, new ValueOrBinding<bool>(true).And(left));
            AssertValue(false, left.And(new ValueOrBinding<bool>(false)));
            AssertBinding(leftBinding, left.And(new ValueOrBinding<bool>(true)));
            AssertValue(true, new ValueOrBinding<bool>(true).Or(left));
            AssertBinding(leftBinding, new ValueOrBinding<bool>(false).Or(left));
            AssertValue(true, left.Or(new ValueOrBinding<bool>(true)));
            AssertBinding(leftBinding, left.Or(new ValueOrBinding<bool>(false)));

            var and = left.And(right);
            var or = left.Or(right);
            Assert.IsFalse(and.Evaluate(control));
            Assert.IsTrue(or.Evaluate(control));
            Assert.AreSame(and.BindingOrDefault, left.And(right).BindingOrDefault);
            Assert.AreSame(or.BindingOrDefault, left.Or(right).BindingOrDefault);
        }

        [TestMethod]
        public void Select_MapsValuesAndCachesEquivalentBindingMappings()
        {
            var value = new ValueOrBinding<int>(4).Select(i => i * 2);
            var sourceBinding = Binding<int>(nameof(TestViewModel.Number));
            var first = sourceBinding.Select(i => i * 2);
            var same = sourceBinding.Select(i => i * 2);
            var control = CreateControl(new TestViewModel { Number = 4 });

            Assert.AreEqual(8, value.GetValue());
            Assert.AreSame(first, same);
            Assert.AreEqual(8, first.Evaluate(control));
        }

        [TestMethod]
        public void Select_ConstantMappingCreatesEvaluatableBinding()
        {
            var sourceBinding = Binding<int>(nameof(TestViewModel.Number));
            var constant = sourceBinding.Select(_ => "constant");
            var control = CreateControl(new TestViewModel { Number = 4 });

            Assert.AreEqual("constant", constant.Evaluate(control));
        }

        private ValueBindingExpression<T> Binding<T>(string property) => bindings.ValueBinding<T>(property, context);
        private ResourceBindingExpression<T> ResourceBinding<T>(string property) => bindings.ResourceBinding<T>(property, context);
        private ValueOrBinding<T> Bound<T>(string property) => new(Binding<T>(property));

        private static void AssertValue<T>(T expected, ValueOrBinding<T> actual)
        {
            Assert.IsTrue(actual.HasValue);
            Assert.IsFalse(actual.HasBinding);
            Assert.AreEqual(expected, actual.ValueOrDefault);
            Assert.AreEqual(expected, actual.GetValue());
            Assert.AreEqual(expected, actual.BoxedValue);
            Assert.IsNull(actual.BindingOrDefault);
        }

        private static void AssertBinding<T>(IBinding expected, ValueOrBinding<T> actual)
        {
            Assert.IsTrue(actual.HasBinding);
            Assert.IsFalse(actual.HasValue);
            Assert.AreSame(expected, actual.BindingOrDefault);
            Assert.AreSame(expected, actual.GetBinding());
            Assert.IsNull(actual.BoxedValue);
        }

        private HtmlGenericControl CreateControl(TestViewModel viewModel)
        {
            var control = new HtmlGenericControl("div") { DataContext = viewModel };
            control.SetDataContextType(context);
            return control;
        }

        private sealed class TestViewModel
        {
            public bool Flag { get; set; } = true;
            public bool OtherFlag { get; set; }
            public bool? NullableFlag { get; set; }
            public int Number { get; set; } = 2;
            public string Text { get; set; } = "text";
            public string? NullableText { get; set; }
            public GridViewDataSet<int> DataSet { get; set; } = new() { Items = new List<int> { 1, 2 } };
            public IGridViewDataSet<int> InterfaceDataSet { get; set; } = new GridViewDataSet<int>();
            public IPageableGridViewDataSet<PagingOptions> InterfacePageableDataSet { get; set; } = new GridViewDataSet<int>();
        }

        private enum TestEnum
        {
            FirstValue
        }
    }
}
