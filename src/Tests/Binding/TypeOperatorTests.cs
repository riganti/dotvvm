using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class TypeOperatorTests
    {
        readonly BindingTestHelper helper = new BindingTestHelper(DotvvmTestHelper.DefaultConfig);
        const string Derived = "DotVVM.Framework.Tests.Binding.BindingCastDerived";

        [TestMethod]
        public void TypeOperators_ServerNullAndAssignability()
        {
            var model = new BindingCastModel();
            Assert.AreEqual(false, helper.ExecuteBinding<object>($"Value is {Derived}", new object[] { model }));
            Assert.IsNull(helper.ExecuteBinding<object>($"Value as {Derived}", new object[] { model }));
            Assert.IsNull(helper.ExecuteBinding<object>($"(Value as {Derived}).Name", new object[] { model }));
            model.Value = new BindingCastBase();
            Assert.AreEqual(false, helper.ExecuteBinding<object>($"Value is {Derived}", new object[] { model }));
            Assert.IsNull(helper.ExecuteBinding<object>($"(Value as {Derived}).Name", new object[] { model }));
            model.Value = new BindingCastDerived { Name = "derived" };
            Assert.AreEqual(true, helper.ExecuteBinding<object>($"Value is {Derived}", new object[] { model }));
            Assert.AreSame(model.Value, helper.ExecuteBinding<object>($"Value as {Derived}", new object[] { model }));
            Assert.AreEqual("derived", helper.ExecuteBinding<object>($"(Value as {Derived}).Name", new object[] { model }));
            Assert.AreEqual(true, helper.ExecuteBinding<object>("Value is DotVVM.Framework.Tests.Binding.IBindingCast", new object[] { model }));
        }

        [TestMethod]
        public void TypeOperators_GenericNullableAndConditional()
        {
            Assert.AreEqual(true, helper.ExecuteBinding<object>("_this is System.Collections.Generic.List<string>", new object[] { new List<string>() }));
            Assert.AreEqual(42, helper.ExecuteBinding<object>("_this as int?", new object[] { 42 }, DataContextStack.Create(typeof(object))));
            Assert.AreEqual(41, helper.ExecuteBinding<object>("_this as int? - 1", new object[] { 42 }, DataContextStack.Create(typeof(object))));
            Assert.AreEqual(-1, helper.ExecuteBinding<object>("_this is int ? -1 : 1", new object[] { 42 }));
            Assert.AreEqual("yes", helper.ExecuteBinding<object>($"Value is {Derived} ? 'yes' : 'no'",
                new object[] { new BindingCastModel { Value = new BindingCastDerived() } }));
        }

        [DataTestMethod]
        [DataRow("Value as int")]
        [DataRow("Value as void")]
        [DataRow("Value is void")]
        [DataRow("Value as Value")]
        [DataRow("Value as string")]
        [DataRow("Value is MissingType")]
        [DataRow("Value as")]
        [DataRow("Value is 42")]
        public void TypeOperators_InvalidTargets(string expression)
        {
            var exception = Xunit.Assert.ThrowsAny<Exception>(() =>
                helper.ParseBinding(expression, DataContextStack.Create(typeof(BindingCastModel))));
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void TypeOperators_OperandEvaluatedOnce()
        {
            var model = new BindingCastModel { Value = new BindingCastDerived { Name = "once" } };
            Assert.AreEqual("once", helper.ExecuteBinding<object>($"(GetValue() as {Derived}).Name", new object[] { model }));
            Assert.AreEqual(1, model.Calls);
            Assert.AreEqual(true, helper.ExecuteBinding<object>($"GetValue() is {Derived}", new object[] { model }));
            Assert.AreEqual(2, model.Calls);
        }

        [TestMethod]
        public void TypeOperators_CompiledJavascriptAndRenamedProperty()
        {
            var contexts = new[] { typeof(BindingCastModel) };
            var hash = typeof(BindingCastDerived).GetTypeHash();
            Assert.AreEqual($"dotvvm.metadata.isType(Value(),\"{hash}\")",
                helper.ValueBindingToJs($"Value is {Derived}", contexts, niceMode: false));
            Assert.AreEqual($"dotvvm.metadata.asType(Value(),\"{hash}\")",
                helper.ValueBindingToJs($"Value as {Derived}", contexts, niceMode: false));
            Assert.AreEqual($"dotvvm.metadata.asType(Value(),\"{hash}\").renamed",
                helper.ValueBindingToJs($"(Value as {Derived}).Name", contexts, niceMode: false));
            Assert.AreEqual($"dotvvm.metadata.asType(Value(),\"{hash}\").renamed(\"updated\").renamed",
                helper.ValueBindingToJs($"(Value as {Derived}).Name = 'updated'", contexts, niceMode: false));
            Assert.AreEqual($"dotvvm.metadata.asType(Value(),\"{hash}\")?.renamed",
                helper.ValueBindingToJs($"(Value as {Derived}).Name", contexts, niceMode: false, nullChecks: true));
        }

        [TestMethod]
        public void TypeOperators_AnnotationsDoNotMutateOperand()
        {
            var context = DataContextStack.Create(typeof(BindingCastBase));
            var visitor = new JavascriptTranslationVisitor(context, DotvvmTestHelper.DefaultConfig.Markup.JavascriptTranslator.MethodCollection);
            var source = Expression.Parameter(typeof(BindingCastBase), "source");
            var cast = visitor.Translate(Expression.TypeAs(source, typeof(BindingCastDerived)));
            Assert.AreEqual(typeof(BindingCastDerived), cast.Annotation<ViewModelInfoAnnotation>().Type);
            Assert.IsNull(visitor.Translate(source).Annotation<ViewModelInfoAnnotation>());
            Assert.AreEqual("source", visitor.Translate(Expression.Convert(source, typeof(object))).FormatScript());
        }

        [TestMethod]
        public void TypeOperators_ValueResourceCommandAndTwoWayUpdate()
        {
            var model = new BindingCastModel { Value = new BindingCastDerived { Name = "initial" } };
            var control = new PlaceHolder { DataContext = model };
            var contexts = new[] { typeof(BindingCastModel) };
            var expression = $"(Value as {Derived}).Name";
            var value = helper.ValueBinding<string>(expression, contexts);
            Assert.AreEqual("initial", value.BindingDelegate(control));
            Assert.AreEqual("initial", helper.ResourceBinding<string>(expression, contexts).BindingDelegate(control));
            value.UpdateDelegate(control, "updated");
            Assert.AreEqual("updated", ((BindingCastDerived)model.Value).Name);
            var command = (Command)helper.Command($"{expression} = 'command'", contexts).BindingDelegate(control);
            command().GetAwaiter().GetResult();
            Assert.AreEqual("command", ((BindingCastDerived)model.Value).Name);
            model.Value = new BindingCastBase();
            Assert.IsNull(value.BindingDelegate(control));
            Assert.ThrowsException<NullReferenceException>(() => value.UpdateDelegate(control, "ignored"));
        }

        [TestMethod]
        public void TypeOperators_StaticCommandUsesStateAndObservableSetter()
        {
            var contexts = new[] { typeof(BindingCastModel) };
            var hash = typeof(BindingCastDerived).GetTypeHash();
            var binding = helper.StaticCommand($"(Value as {Derived}).Name = 'updated'", contexts);
            var js = BindingTestHelper.GetStaticCommandJavascriptBody(binding);
            StringAssert.Contains(js, $"dotvvm.metadata.asType");
            StringAssert.Contains(js, hash);
            StringAssert.Contains(js, "renamed(\"updated\")");
            var typeTest = helper.StaticCommand($"Value is {Derived}", contexts);
            var typeTestJs = BindingTestHelper.GetStaticCommandJavascriptBody(typeTest);
            StringAssert.Contains(typeTestJs, "dotvvm.metadata.isType");
        }

        [TestMethod]
        public void TypeOperators_PlainObjectsAndSingleEvaluationJavascript()
        {
            var config = DotvvmTestHelper.CreateConfiguration();
            config.Markup.JavascriptTranslator.MethodCollection.AddMethodTranslator(
                () => default(BindingCastModel).GetValue(),
                new GenericMethodCompiler(args => new JsIdentifierExpression("getPlain").Invoke()
                    .WithAnnotation(new ViewModelInfoAnnotation(typeof(BindingCastBase), containsObservables: false))));
            var customHelper = new BindingTestHelper(config);
            var contexts = new[] { typeof(BindingCastModel) };
            var hash = typeof(BindingCastDerived).GetTypeHash();
            Assert.AreEqual($"dotvvm.metadata.asType(getPlain(),\"{hash}\")?.renamed",
                customHelper.ValueBindingToJs($"(GetValue() as {Derived}).Name", contexts, niceMode: false, nullChecks: true));
            Assert.AreEqual($"dotvvm.metadata.isType(getPlain(),\"{hash}\")",
                customHelper.ValueBindingToJs($"GetValue() is {Derived}", contexts, niceMode: false, nullChecks: true));
        }

        [TestMethod]
        public void TypeOperators_DerivedProtectionIsRespected()
        {
            var exception = Xunit.Assert.ThrowsAny<Exception>(() =>
                helper.ValueBindingToJs($"(Value as {Derived}).Secret", new[] { typeof(BindingCastModel) }));
            Assert.IsTrue(exception.AllInnerExceptions().Any(e => e.Message.Contains("encrypted")));
        }
    }

    public interface IBindingCast { }
    public class BindingCastBase { }
    public class BindingCastDerived : BindingCastBase, IBindingCast
    {
        [JsonPropertyName("renamed")]
        public string Name { get; set; }
        [Protect(ProtectMode.EncryptData)]
        public string Secret { get; set; }
    }
    public class BindingCastModel
    {
        public BindingCastBase Value { get; set; }
        public int Calls;
        public BindingCastBase GetValue() { Calls++; return Value; }
    }
}
