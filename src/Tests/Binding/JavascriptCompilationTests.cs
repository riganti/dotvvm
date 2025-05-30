using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Configuration;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class JavascriptCompilationTests
    {
        private static readonly DotvvmConfiguration configuration;
        private static readonly BindingTestHelper bindingHelper;

        static JavascriptCompilationTests()
        {
            configuration = DotvvmTestHelper.CreateConfiguration();
            configuration.RegisterApiClient(typeof(TestApiClient), "http://server/api", "./apiscript.js", "_testApi");
            var methods = configuration.Markup.JavascriptTranslator.MethodCollection;
            methods.AddMethodTranslator(() => TestJsTransations.GetTestPlainObject(), new GenericMethodCompiler(args => new JsIdentifierExpression("testPlainObject").WithAnnotation(new ViewModelInfoAnnotation(typeof(TestArraysViewModel), containsObservables: false))));
            methods.AddMethodTranslator(() => TestJsTransations.GetTestObjectWithObservables(), new GenericMethodCompiler(args => new JsIdentifierExpression("testPlainObject").WithAnnotation(new ViewModelInfoAnnotation(typeof(TestArraysViewModel), containsObservables: true))));
            configuration.Freeze();
            bindingHelper = new BindingTestHelper(configuration);
        }
        public string CompileBinding(string expression, params Type[] contexts) => CompileBinding(expression, contexts, expectedType: typeof(object));
        public string CompileBinding(string expression, NamespaceImport[] imports, params Type[] contexts) => CompileBinding(expression, contexts, expectedType: typeof(object), imports);
        public string CompileBinding(string expression, Type[] contexts, Type expectedType, NamespaceImport[] imports = null, bool nullChecks = false)
        {
            return bindingHelper.ValueBindingToJs(expression, contexts, expectedType, imports, nullChecks, niceMode: false);
        }

        public static string FormatKnockoutScript(ParametrizedCode code) => JavascriptTranslator.FormatKnockoutScript(code);

        public string CompileBinding(Func<Dictionary<string, Expression>, Expression> expr, Type[] contexts)
        {
            var context = DataContextStack.Create(contexts.FirstOrDefault() ?? typeof(object), extensionParameters: new BindingExtensionParameter[]{
                new BindingPageInfoExtensionParameter()
                });
            for (int i = 1; i < contexts.Length; i++)
            {
                context = DataContextStack.Create(contexts[i], context);
            }
            var expressionTree = expr(BindingExpressionBuilder.GetParameters(context).ToDictionary(e => e.Name, e => (Expression)e));
            var configuration = DotvvmTestHelper.DefaultConfig;
            var jsExpression = new JsParenthesizedExpression(configuration.ServiceProvider.GetRequiredService<JavascriptTranslator>().CompileToJavascript(expressionTree, context));
            jsExpression.AcceptVisitor(new KnockoutObservableHandlingVisitor(true));
            JsTemporaryVariableResolver.ResolveVariables(jsExpression);
            return JavascriptTranslator.FormatKnockoutScript(jsExpression);
        }

        [DataTestMethod]
        [DataRow("_this == 'Local'", "$data==\"Local\"")]
        [DataRow("'Local' == _this", "\"Local\"==$data")]
        [DataRow("_this == 2", "$data==\"Local\"")]
        [DataRow("2 == _this", "\"Local\"==$data")]
        [DataRow("_this & 'Local'", "dotvvm.translations.enums.fromInt(dotvvm.translations.enums.toInt($data,\"bmCpvOUHn4NxUNdc\")&2,\"bmCpvOUHn4NxUNdc\")")]
        [DataRow("'Local' & _this", "dotvvm.translations.enums.fromInt(2&dotvvm.translations.enums.toInt($data,\"bmCpvOUHn4NxUNdc\"),\"bmCpvOUHn4NxUNdc\")")]
        [DataRow("_this | 'Local'", "dotvvm.translations.enums.fromInt(dotvvm.translations.enums.toInt($data,\"bmCpvOUHn4NxUNdc\")|2,\"bmCpvOUHn4NxUNdc\")")]
        [DataRow("_this & 1", "dotvvm.translations.enums.fromInt(dotvvm.translations.enums.toInt($data,\"bmCpvOUHn4NxUNdc\")&1,\"bmCpvOUHn4NxUNdc\")")]
        [DataRow("1 & _this", "dotvvm.translations.enums.fromInt(1&dotvvm.translations.enums.toInt($data,\"bmCpvOUHn4NxUNdc\"),\"bmCpvOUHn4NxUNdc\")")]
        public void JavascriptCompilation_EnumOperators(string expr, string expectedJs)
        {
            var js = CompileBinding(expr, typeof(OurDateTimeKind));
            Assert.AreEqual(expectedJs, js);
        }

        enum OurDateTimeKind
        {
            Unspecified = 0,
            Utc = 1,
            Local = 2,
        }

        [DataTestMethod]
        [DataRow("StringProp + StringProp", "(StringProp()??\"\")+(StringProp()??\"\")")]
        [DataRow("StringProp + null", "StringProp()??\"\"")]
        [DataRow("null + StringProp", "StringProp()??\"\"")]
        [DataRow("'' + StringProp", "StringProp()??\"\"")]
        [DataRow("BoolProp + StringProp", "BoolProp()+(StringProp()??\"\")")]
        [DataRow("IntProp + IntProp + 'aa'", "IntProp()+IntProp()+\"aa\"")]
        [DataRow("DoubleProp + 'aa'", "DoubleProp()+\"aa\"")]
        [DataRow("'a' + DoubleProp", "\"a\"+DoubleProp()")]
        [DataRow("'a' + NullableIntProp + null", "\"a\"+(NullableIntProp()??\"\")")]
        public void JavascriptCompilation_StringPlus(string expr, string expectedJs)
        {
            var js = CompileBinding(expr, typeof(TestViewModel));
            Assert.AreEqual(expectedJs, js);
        }

        [DataTestMethod]
        [DataRow("NullableIntProp + NullableDoubleProp", "NullableIntProp()+NullableDoubleProp()")]
        [DataRow("NullableIntProp & NullableIntProp", "NullableIntProp()&NullableIntProp()")]
        [DataRow("NullableIntProp | NullableIntProp", "NullableIntProp()|NullableIntProp()")]
        [DataRow("NullableIntProp ^ NullableIntProp", "NullableIntProp()^NullableIntProp()")]
        [DataRow("NullableIntProp + 10", "NullableIntProp()+10")]
        [DataRow("NullableIntProp - 10L", "NullableIntProp()-10")]
        [DataRow("NullableIntProp / 10.0", "NullableIntProp()/10")]
        [DataRow("10.0 / NullableIntProp", "10/NullableIntProp()")]
        [DataRow("null / NullableIntProp", "null/NullableIntProp()")]
        [DataRow("null == NullableIntProp", "null==NullableIntProp()")]
        [DataRow("10 > NullableIntProp", "10>NullableIntProp()")]
        [DataRow("NullableDoubleProp < 10", "NullableDoubleProp()<10")]
        [DataRow("10+null", "10+null")]
        [DataRow("10+null", "10+null")]
        public void JavascriptCompilation_NullableOps(string expr, string expectedJs)
        {
            var js = CompileBinding(expr, typeof(TestViewModel));
            Assert.AreEqual(expectedJs, js);
        }

        [TestMethod]
        public void JavascriptCompilation_ConstantToString()
        {
            var js = CompileBinding("5", Type.EmptyTypes, typeof(string));
            Assert.AreEqual("\"5\"", js);
        }

        [TestMethod]
        public void JavascriptCompilation_ToString()
        {
            var js = CompileBinding("MyProperty", new[] { typeof(TestViewModel2) }, typeof(string));
            Assert.AreEqual("dotvvm.globalize.bindingNumberToString(MyProperty)", js);
        }

        [TestMethod]
        public void JavascriptCompilation_ToString_Invalid()
        {
            Assert.ThrowsException<NotSupportedException>(() => {
                var js = CompileBinding("TestViewModel2", new[] { typeof(TestViewModel) }, typeof(string));
            });
        }

        [TestMethod]
        [DataRow(@"$""Interpolated {StringProp} {StringProp}""")]
        [DataRow(@"$'Interpolated {StringProp} {StringProp}'")]
        public void JavascriptCompilation_InterpolatedString(string expression)
        {
            var js = CompileBinding(expression, new[] { typeof(TestViewModel) }, typeof(string));
            Assert.AreEqual("dotvvm.translations.string.format(\"Interpolated {0} {1}\",[StringProp(),StringProp()])", js);
        }

        [TestMethod]
        public void JavascriptCompilation_InterpolatedString_NoExpressions()
        {
            var js = CompileBinding("$'Non-Interpolated {{ no-expr }}'", new[] { typeof(TestViewModel) });
            Assert.AreEqual("\"Non-Interpolated { no-expr }\"", js);
        }

        [TestMethod]
        public void JavascriptCompilation_UnwrappedObservables()
        {
            var js = CompileBinding("TestViewModel2.Collection[0].StringValue.Length + TestViewModel2.Collection[8].StringValue", new[] { typeof(TestViewModel) });
            Assert.AreEqual("(TestViewModel2().Collection()[0]().StringValue().length??\"\")+(TestViewModel2().Collection()[8]().StringValue()??\"\")", js);
        }

        [TestMethod]
        public void JavascriptCompilation_Parent()
        {
            var js = CompileBinding("_parent + _parent2 + _parent0 + _parent1 + _parent3", typeof(string), typeof(string), typeof(string), typeof(string), typeof(string))
                .Replace("(", "").Replace(")", "");
            Assert.AreEqual("$parent+$parents[1]+$data+$parent+$parents[2]", js);
        }

        [DataTestMethod]
        [DataRow("2+2", "2+2", DisplayName = "2+2")]
        [DataRow("2+2+2", "2+2+2", DisplayName = "2+2+2")]
        [DataRow("(2+2)+2", "2+2+2", DisplayName = "(2+2)+2")]
        [DataRow("2+(2+2)", "2+(2+2)", DisplayName = "2+(2+2)")]
        [DataRow("2+(2*2)", "2+2*2", DisplayName = "2+(2*2)")]
        [DataRow("2*(2+2)", "2*(2+2)", DisplayName = "2*(2+2)")]
        [DataRow("IntProp & (2+2)", "IntProp()&2+2", DisplayName = "IntProp & (2+2)")]
        [DataRow("IntProp & 2+2", "IntProp()&2+2", DisplayName = "IntProp & 2+2")]
        [DataRow("IntProp & -1", "IntProp()&-1", DisplayName = "IntProp & -1")]
        [DataRow("'a' + 'b'", "\"ab\"", DisplayName = "'a' + 'b'")]
        [DataRow("IntProp ^ 1", "IntProp()^1", DisplayName = "IntProp ^ 1")]
        [DataRow("'xx' + IntProp", "\"xx\"+IntProp()", DisplayName = "'xx' + IntProp")]
        [DataRow("true == (IntProp == 1)", "true==(IntProp()==1)", DisplayName = "true == (IntProp == 1)")]
        [DataRow("TestViewModel2 == null", "TestViewModel2()==null", DisplayName = "TestViewModel2 == null")]
        [DataRow("NullableDateOnly == null", "NullableDateOnly()==null", DisplayName = "NullableDateOnly == null")]
        [DataRow("NullableDateOnly == NullableDateOnly", "NullableDateOnly()==NullableDateOnly()", DisplayName = "NullableDateOnly == NullableDateOnly")]
        [DataRow("null != StringProp", "null!=StringProp()", DisplayName = "null != StringProp")]
        [DataRow("(EnumProperty & 2) == 0", "dotvvm.translations.enums.fromInt(dotvvm.translations.enums.toInt(EnumProperty(),\"nEayAzHQ5xyCfSP6\")&2,\"nEayAzHQ5xyCfSP6\")==\"A\"", DisplayName = "(EnumProperty & 2) == 0")]
        [DataRow("EnumProperty == 'B'", "EnumProperty()==\"B\"", DisplayName = "EnumProperty & 2 == 0")]
        public void JavascriptCompilation_BinaryExpressions(string expr, string expectedJs)
        {
            var js = CompileBinding(expr, new [] { typeof(TestViewModel) });
            Assert.AreEqual(expectedJs, js);
        }

        [TestMethod]
        public void JavascriptCompilation_ExclusiveOr_ReturnsBooleanIfOperandsAreBooleans()
        {
            var js = CompileBinding("BoolProp = BoolProp ^ true", new[] { typeof(TestViewModel) });
            Assert.AreEqual("BoolProp(BoolProp()!=true).BoolProp", js);
        }

        [TestMethod]
        public void JavascriptCompilation_OnesComplementOperator()
        {
            var js = CompileBinding("IntProp = ~IntProp", new[] { typeof(TestViewModel) });
            Assert.AreEqual("IntProp(~IntProp()).IntProp", js);
        }

        [TestMethod]
        public void JavascriptCompilation_BindingPageInfo_IsPostbackRunning()
        {
            var js = CompileBinding("_page.IsPostbackRunning");
            Assert.AreEqual("dotvvm.isPostbackRunning()", js);
        }

        [TestMethod]
        public void JavascriptCompilation_BindingPageInfo_EvaluatingOnClient()
        {
            var js = CompileBinding("_page.EvaluatingOnClient");
            Assert.AreEqual("true", js);
        }

        [TestMethod]
        public void JavascriptCompilation_BindingPageInfo_EvaluatingOnServer()
        {
            var js = CompileBinding("_page.EvaluatingOnServer");
            Assert.AreEqual("false", js);
        }

        [TestMethod]
        public void JavascriptCompilation_NullableDateExpression()
        {
            var result = CompileBinding("DateFrom == null || DateTo == null || DateFrom.Value <= DateTo.Value", typeof(TestViewModel));
            Assert.AreEqual("DateFrom()==null||DateTo()==null||DateFrom()<=DateTo()", result);
            var result2 = CompileBinding("DateFrom == null || DateTo == null || DateFrom <= DateTo", typeof(TestViewModel));
            Assert.AreEqual("DateFrom()==null||DateTo()==null||DateFrom()<=DateTo()", result2);
        }

        [TestMethod]
        public void JavascriptCompilation_LambdaExpression()
        {
            var funcP = Expression.Parameter(typeof(string), "parameter");
            var blockLocal = Expression.Parameter(typeof(int), "local");
            var result = CompileBinding(p =>
                Expression.Lambda(
                    Expression.Block(
                        new [] { blockLocal },
                        Expression.Assign(blockLocal, Expression.Add(Expression.Constant(6), p["_this"])),
                        blockLocal
                    ),
                    new [] { funcP }
                ),
                new [] {
                    typeof(int)
                }
            );
            Assert.AreEqual("((parameter)=>(local=6+$data,local))", result);
        }

        [TestMethod]
        public void JavascriptCompilation_BlockExpression()
        {
            var funcP = Expression.Parameter(typeof(string), "parameter");
            var blockLocal = Expression.Parameter(typeof(int), "local");
            var result = CompileBinding(p =>
                Expression.Block(
                    new [] { blockLocal },
                    Expression.Assign(blockLocal, Expression.Add(Expression.Constant(6), p["_this"])),
                    blockLocal
                ),
                new [] {
                    typeof(int)
                }
            );
            Assert.AreEqual("(local=6+$data,local)", result);
        }

        [TestMethod]
        public void JavascriptCompilation_Api_GetFunction()
        {
            var result = CompileBinding("_testApi.GetString()");
            Assert.AreEqual("dotvvm.api.invoke(dotvvm.api._testApi,\"getString\",function(){return [];},function(args){return [dotvvm.eventHub.get(\"DotVVM.Framework.Tests.Binding.TestApiClient/\")];},function(args){return [];},null,function(args){return \"\";},$element)", result);
            var assignment = CompileBinding("StringProp = _testApi.GetString()", typeof(TestViewModel));
            Assert.AreEqual("StringProp(dotvvm.api.invoke(dotvvm.api._testApi,\"getString\",function(){return [];},function(args){return [dotvvm.eventHub.get(\"DotVVM.Framework.Tests.Binding.TestApiClient/\")];},function(args){return [];},null,function(args){return \"\";},$element)()).StringProp", assignment);
        }

        [TestMethod]
        public void JavascriptCompilation_Api_GetDate()
        {
            var result = CompileBinding("_testApi.GetCurrentTime('test')");
            Assert.AreEqual("dotvvm.api.invoke(dotvvm.api._testApi,\"getCurrentTime\",function(){return [\"test\"];},function(args){return [dotvvm.eventHub.get(\"DotVVM.Framework.Tests.Binding.TestApiClient/\")];},function(args){return [];},null,function(args){return \"\";},$element)", result);
            var assignment = CompileBinding("DateFrom = _testApi.GetCurrentTime('test')", typeof(TestViewModel));
            Assert.AreEqual("DateFrom(dotvvm.serialization.serializeDate(dotvvm.api.invoke(dotvvm.api._testApi,\"getCurrentTime\",function(){return [\"test\"];},function(args){return [dotvvm.eventHub.get(\"DotVVM.Framework.Tests.Binding.TestApiClient/\")];},function(args){return [];},null,function(args){return \"\";},$element)(),false)).DateFrom", assignment);
        }

        [TestMethod]
        public void JavascriptCompilation_Api_DateParameter()
        {
            var result = CompileBinding("_testApi.PostDateToString(DateFrom.Value)", typeof(TestViewModel));
            Assert.IsTrue(result.StartsWith("dotvvm.api.invoke(dotvvm.api._testApi,\"postDateToString\",function(){return [dotvvm.serialization.parseDate(DateFrom(),true)];},function(args){return [];},function(args){return [\"DotVVM.Framework.Tests.Binding.TestApiClient/\"];},$element,function(args){return \""));
            Assert.IsTrue(result.EndsWith("\";},$element)"));
        }

        [TestMethod]
        public void JavascriptCompilation_WrappedIdentifierExpression()
        {
            var result = bindingHelper.ValueBinding<object>("_this", new [] {typeof(TestViewModel) });
            Assert.AreEqual("$data", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("$rawData", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("$rawData", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }

        [TestMethod]
        public void JavascriptCompilation_BindingDateToString()
        {
            var result = bindingHelper.ValueBinding<object>("_this.ToString()", new [] { typeof(DateTime) });
            Assert.AreEqual("dotvvm.globalize.bindingDateToString($rawData)()", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("dotvvm.globalize.bindingDateToString($rawData)", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("dotvvm.globalize.bindingDateToString($rawData)", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }

        [TestMethod]
        public void JavascriptCompilation_ParentReference()
        {
            var result = bindingHelper.ValueBinding<object>("_parent", new [] {typeof(TestViewModel), typeof(string) });
            Assert.AreEqual("$parent", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("$parentContext.$rawData", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("$parentContext.$rawData", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }
        [TestMethod]
        public void JavascriptCompilation_ParentPropertyReference()
        {
            var result = bindingHelper.ValueBinding<object>("_parent.StringProp", new [] {typeof(TestViewModel), typeof(string) });
            Assert.AreEqual("$parent.StringProp()", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("$parent.StringProp", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("$parent.StringProp", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }

        [TestMethod]
        public void JavascriptCompilation_WrappedPropertyAccessExpression()
        {
            var result = bindingHelper.ValueBinding<object>("StringProp", new [] {typeof(TestViewModel) });
            Assert.AreEqual("StringProp()", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("StringProp", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("StringProp", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }

        [TestMethod]
        public void JavascriptCompilation_WrappedNestedPropertyAccessExpression()
        {
            var result = bindingHelper.ValueBinding<object>("TestViewModel2.SomeString", new[] { typeof(TestViewModel) });
            Assert.AreEqual("TestViewModel2()?.SomeString()", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("TestViewModel2()?.SomeString", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("dotvvm.evaluator.wrapObservable(()=>TestViewModel2()?.SomeString)", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }

        [TestMethod]
        public void JavascriptCompilation_WrappedNestedListAccessExpression()
        {
            var result = bindingHelper.ValueBinding<object>("TestViewModel2.Collection", new[] { typeof(TestViewModel) });
            Assert.AreEqual("TestViewModel2()?.Collection()", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("TestViewModel2()?.Collection", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("dotvvm.evaluator.wrapObservable(()=>TestViewModel2()?.Collection,true)", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }

        [TestMethod]
        public void JavascriptCompilation_WrappedNegatedBooleanAccessExpression()
        {
            var result = bindingHelper.ValueBinding<object>("!Value", new[] { typeof(Something) });
            Assert.AreEqual("!Value()", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("!Value()", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("ko.pureComputed(()=>!Value())", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }

        [TestMethod]
        public void JavascriptCompilation_WrappedExpression()
        {
            var result = bindingHelper.ValueBinding<object>("StringProp.Length + 43", new [] {typeof(TestViewModel) });
            Assert.AreEqual("StringProp()?.length+43", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("StringProp()?.length+43", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("ko.pureComputed(()=>StringProp()?.length+43)", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }

        [TestMethod]
        public void JavascriptCompilation_FormatStringExpression()
        {
            var result = bindingHelper.ValueBinding<object>("LongProperty.ToString('0000')", new [] {typeof(TestViewModel) });
            Assert.AreEqual("dotvvm.globalize.bindingNumberToString(LongProperty,\"0000\")()", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("dotvvm.globalize.bindingNumberToString(LongProperty,\"0000\")", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("dotvvm.globalize.bindingNumberToString(LongProperty,\"0000\")", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }


        [TestMethod]
        public void JsTranslator_SimpleCSharpExpression()
        {
            Expression<Func<string, string>> expr = abc => abc + "def";
            var tree = configuration.ServiceProvider.GetRequiredService<JavascriptTranslator>().CompileToJavascript(expr, DataContextStack.Create(typeof(object)));
            Assert.AreEqual("(abc) => abc + \"def\"", tree.ToString());
        }

        [TestMethod]
        public void JsTranslator_StaticFieldInCSharpExpression()
        {
            Expression<Func<string, string>> expr = _ => string.Empty;
            var tree = configuration.ServiceProvider.GetRequiredService<JavascriptTranslator>().CompileToJavascript(expr, DataContextStack.Create(typeof(object)));
            Assert.AreEqual("(_) => \"\"", tree.ToString());
        }

        [TestMethod]
        public void JsTranslator_LambdaWithParameter()
        {
            var result = this.CompileBinding("_this + arg", new [] { typeof(string) }, typeof(Func<string, string>));
            Assert.AreEqual("(arg)=>$data+ko.unwrap(arg)", result);
        }

        [TestMethod]
        public void JsTranslator_LambdaWithDelegateInvocation()
        {
            var result = this.CompileBinding("arg(12) + _this", new [] { typeof(string) }, typeof(Func<Func<int, string>, string>));
            Assert.AreEqual("(arg)=>(ko.unwrap(arg)(12)??\"\")+$data", result);
        }

        [TestMethod]
        public void JsTranslator_EnumToString()
        {
            var result = CompileBinding("EnumProperty.ToString()", typeof(TestViewModel));
            var resultImplicit = CompileBinding("EnumProperty", new [] { typeof(TestViewModel) }, typeof(string));

            Assert.AreEqual(result, resultImplicit);
            Assert.AreEqual("EnumProperty", result);
        }

        [DataTestMethod]
        [DataRow("EnumProperty = IntProp", "EnumProperty(dotvvm.translations.enums.fromInt(IntProp(),\"nEayAzHQ5xyCfSP6\")).EnumProperty", DisplayName = "EnumProperty = IntProp")]
        [DataRow("EnumProperty & TestEnum.B", "dotvvm.translations.enums.fromInt(dotvvm.translations.enums.toInt(EnumProperty(),\"nEayAzHQ5xyCfSP6\")&1,\"nEayAzHQ5xyCfSP6\")", DisplayName = "EnumProperty & TestEnum.B")]
        [DataRow("EnumProperty + 1", "dotvvm.translations.enums.fromInt(dotvvm.translations.enums.toInt(EnumProperty(),\"nEayAzHQ5xyCfSP6\")+1,\"nEayAzHQ5xyCfSP6\")", DisplayName = "EnumProperty + 1")]
        public void JavascriptCompilation_EnumOperations(string expr, string expectedJs)
        {
            var js = CompileBinding(expr, new [] { typeof(TestViewModel) });
            Assert.AreEqual(expectedJs, js);
        }


        [TestMethod]
        public void JsTranslator_DataContextShift()
        {
            var result = bindingHelper.ValueBinding<object>("_this.StringProp", new [] { typeof(TestViewModel) });
            var expr0 = JavascriptTranslator.FormatKnockoutScript(result.KnockoutExpression, dataContextLevel: 0);
            var expr0_explicit = JavascriptTranslator.FormatKnockoutScript(result.KnockoutExpression, allowDataGlobal: false, dataContextLevel: 0);
            var expr1 = JavascriptTranslator.FormatKnockoutScript(result.KnockoutExpression, dataContextLevel: 1);
            var expr2 = JavascriptTranslator.FormatKnockoutScript(result.KnockoutExpression, dataContextLevel: 2);
            Assert.AreEqual("StringProp", expr0);
            Assert.AreEqual("$data.StringProp", expr0_explicit);
            Assert.AreEqual("$parent.StringProp", expr1);
            Assert.AreEqual("$parents[1].StringProp", expr2);
        }

        [TestMethod]
        public void JsTranslator_IntegerArithmetic()
        {
            var result = CompileBinding("IntProp / 2 + (IntProp + 1) / (IntProp - 1)", typeof(TestViewModel));
            Assert.AreEqual("(IntProp()/2|0)+((IntProp()+1)/(IntProp()-1)|0)", result);
        }

        [TestMethod]
        public void JsTranslator_BooleanOperatorPrecedence()
        {
            var result = CompileBinding("IntProp > 0 && IntProp > 1 || IntProp > 2 && IntProp > 3", new [] { typeof(TestViewModel)});
            Assert.AreEqual("IntProp()>0&&IntProp()>1||IntProp()>2&&IntProp()>3", result);
        }

        [TestMethod]
        public void JsTranslator_ArrayIndexer()
        {
            var result = CompileBinding("LongArray[1] == 3 && VmArray[0].MyProperty == 1 && VmArray.Length > 1", new [] { typeof(TestViewModel)});
            Assert.AreEqual("LongArray()[1]()==3&&VmArray()[0]().MyProperty()==1&&VmArray().length>1", result);
        }

        [TestMethod]
        public void JsTranslator_ArrayElement_Get()
        {
            var result = CompileBinding("Array[1]", typeof(TestViewModel5));
            Assert.AreEqual("Array()[1]", result);
        }

        [TestMethod]
        public void JsTranslator_ReadOnlyArrayElement_Get()
        {
            var result = CompileBinding("ReadOnlyArray[1]", typeof(TestViewModel5));
            Assert.AreEqual("ReadOnlyArray()[1]", result);
        }

        [TestMethod]
        public void JsTranslator_ArrayElement_Set()
        {
            var result = CompileBinding("Array[1] = 123", new[] { typeof(TestViewModel5) }, typeof(void));
            Assert.AreEqual("dotvvm.translations.array.setItem(Array,1,123)", result);
        }

        [TestMethod]
        public void JsTranslator_ListIndexer_Get()
        {
            var result = CompileBinding("List[1]", typeof(TestViewModel5));
            Assert.AreEqual("List()[1]", result);
        }

        [TestMethod]
        public void JsTranslator_ReadOnlyListIndexer_Get()
        {
            var result = CompileBinding("List.AsReadOnly()[1]", typeof(TestViewModel5));
            Assert.AreEqual("List()[1]", result);
        }

        [TestMethod]
        public void JsTranslator_ListIndexer_Set()
        {
            var result = CompileBinding("List[1] = 123", new[] { typeof(TestViewModel5) }, typeof(void));
            Assert.AreEqual("dotvvm.translations.array.setItem(List,1,123)", result);
        }

        [TestMethod]
        public void JsTranslator_DictionaryIndexer_Get()
        {
            var result = CompileBinding("Dictionary[1]", typeof(TestViewModel5));
            Assert.AreEqual("dotvvm.translations.dictionary.getItem(Dictionary(),1)", result);
        }

        [TestMethod]
        public void JsTranslator_DictionaryIndexer_GetObject()
        {
            var result = CompileBinding("StringVmDictionary['test'].Collection[5].StringValue.Length", [typeof(TestViewModel)], typeof(object), nullChecks: true);
            Assert.AreEqual("(()=>{let a;return ((a=dotvvm.translations.dictionary.getItem(StringVmDictionary(),\"test\")?.Collection())&&a[5]())?.StringValue()?.length;})()", result);
        }

        [TestMethod]
        public void JsTranslator_ReadOnlyDictionaryIndexer_Get()
        {
            var result = CompileBinding("ReadOnlyDictionary[1]", typeof(TestViewModel5));
            Assert.AreEqual("dotvvm.translations.dictionary.getItem(ReadOnlyDictionary(),1)", result);
        }

        [DataTestMethod]
        [DataRow("Dictionary")]
        [DataRow("ReadOnlyDictionary")]
        public void JsTranslator_Dictionary_GetValueOrDefault(string property)
        {
            var imports = new NamespaceImport[] { new("System.Collections.Generic"), new("DotVVM.Framework.Utils") };
            var result = CompileBinding($"{property}.GetValueOrDefault(1)", imports, typeof(TestViewModel5));
            Assert.AreEqual($"dotvvm.translations.dictionary.getItem({property}(),1,0)", result);
            var result2 = CompileBinding($"{property}.GetValueOrDefault(1, 1024)", imports, typeof(TestViewModel5));
            Assert.AreEqual($"dotvvm.translations.dictionary.getItem({property}(),1,1024)", result2);
        }

        [TestMethod]
        public void JsTranslator_DictionaryIndexer_Set()
        {
            var result = CompileBinding("Dictionary[1] = 123", new[] { typeof(TestViewModel5) }, typeof(void));
            Assert.AreEqual("dotvvm.translations.dictionary.setItem(Dictionary,1,123)", result);
        }

        [TestMethod]
        public void JsTranslator_DictionaryClear()
        {
            var result = CompileBinding("Dictionary.Clear()", new[] { typeof(TestViewModel5) }, typeof(void));
            Assert.AreEqual("dotvvm.translations.dictionary.clear(Dictionary)", result);
        }

        [TestMethod]
        public void JsTranslator_DictionaryContainsKey()
        {
            var result = CompileBinding("Dictionary.ContainsKey(123)", new[] { typeof(TestViewModel5) }, typeof(bool));
            Assert.AreEqual("dotvvm.translations.dictionary.containsKey(Dictionary(),123)", result);
        }

        [TestMethod]
        public void JsTranslator_DictionaryRemove()
        {
            var result = CompileBinding("Dictionary.Remove(123)", new[] { typeof(TestViewModel5) }, typeof(bool));
            Assert.AreEqual("dotvvm.translations.dictionary.remove(Dictionary,123)", result);
        }

        [TestMethod]
        [DataRow("Enumerable.Where(LongArray, (long item) => item % 2 == 0)", DisplayName = "Regular call of Enumerable.Where")]
        [DataRow("LongArray.Where((long item) => item % 2 == 0)", DisplayName = "Syntax sugar - extension method")]
        [DataRow("LongArray.ToImmutableArray().Where((long item) => item % 2 == 0)", DisplayName = "Immutable array - extension method")]
        public void JsTranslator_EnumerableWhere(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq"), new NamespaceImport("System.Collections.Immutable") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray().filter((item)=>ko.unwrap(item)%2==0)", result);
        }

        [TestMethod]
        public void JsTranslator_NestedEnumerableMethods()
        {
            var result = CompileBinding("Enumerable.Where(Enumerable.Where(LongArray, (long item) => item % 2 == 0), (long item) => item % 3 == 0)",
                new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray().filter((item)=>ko.unwrap(item)%2==0).filter((item)=>ko.unwrap(item)%3==0)", result);
        }

        [TestMethod]
        [DataRow("Enumerable.Select(LongArray, (long item) => -item)", DisplayName = "Regular call of Enumerable.Select")]
        [DataRow("LongArray.Select((long item) => -item)", DisplayName = "Syntax sugar - extension method")]
        [DataRow("LongArray.ToImmutableArray().Select((long item) => -item)", DisplayName = "Immutable array - extension method")]
        [DataRow("LongArray.ToImmutableList().Select((long item) => -item)", DisplayName = "Immutable list - extension method")]
        public void JsTranslator_EnumerableSelect(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq"), new NamespaceImport("System.Collections.Immutable") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray().map((item)=>-ko.unwrap(item))", result);
        }

        [TestMethod]
        public void JsTranslator_EnumerableSelectObservableHandling1()
        {
            var result = CompileBinding("VmArray.Select(item => item.MyProperty).ToArray()[0] == 1", new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("VmArray().map((item)=>ko.unwrap(item).MyProperty())[0]==1", result);
        }

        [TestMethod]
        public void JsTranslator_EnumerableSelectObservableHandling2()
        {
            var result = CompileBinding("VmArray.Select(item => item.MyProperty + 1).ToArray()[0] == 1", new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("VmArray().map((item)=>ko.unwrap(item).MyProperty()+1)[0]==1", result);
        }

        [TestMethod]
        public void JsTranslator_EnumerableSelectObservableHandling3()
        {
            var result = CompileBinding("VmArray.Select(item => item.ChildObject).ToArray()[0].SomeString == 'abc'", new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("VmArray().map((item)=>ko.unwrap(item).ChildObject())[0].SomeString()==\"abc\"", result);
        }

        [TestMethod]
        public void JsTranslator_EnumerableSelectFirstObservableHandling()
        {
            var result = CompileBinding("VmArray.Select(item => item.ChildObject).FirstOrDefault(i => i.SomeString.StartsWith('a')).SomeString == 'abc'", new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("VmArray().map((item)=>ko.unwrap(item).ChildObject()).find((i)=>ko.unwrap(i).SomeString().startsWith(\"a\")).SomeString()==\"abc\"", result);
            //                                                                    ^^^^^        not observable, because .map unwraps it ^^
            // without the select, we'd need observable unwrap after .find
            var withoutSelect = CompileBinding("VmArray.FirstOrDefault(i => i.ChildObject.SomeString.StartsWith('a')).ChildObject.SomeString == 'abc'", new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("VmArray().find((i)=>ko.unwrap(i).ChildObject().SomeString().startsWith(\"a\"))().ChildObject().SomeString()==\"abc\"", withoutSelect);
        }

        [TestMethod]
        public void JsTranslator_EnumerableSelectOrderObservableHandling()
        {
            var result = CompileBinding("VmArray.Select(item => item.ChildObject).OrderBy(i => i.SomeString).ToArray()[0].SomeString == 'abc'", new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("dotvvm.translations.array.orderBy(VmArray().map((item)=>ko.unwrap(item).ChildObject()),(i)=>ko.unwrap(i).SomeString(),null)[0].SomeString()==\"abc\"", result);
            //                                                                                                  not observable, because .map unwraps it ^^^
        }

        [TestMethod]
        public void JsTranslator_EnumerableSelectFilterObservableHandling()
        {
            var result = CompileBinding("VmArray.Select(item => item.ChildObject).Where(i => i != null).ElementAt(1).SomeString == 'abc'", new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("VmArray().map((item)=>ko.unwrap(item).ChildObject()).filter((i)=>ko.unwrap(i)!=null)[1].SomeString()==\"abc\"", result);
            //                                                           not observable, because .map unwraps it ^^^
        }

        [TestMethod]
        public void JsTranslator_EnumerableSelectSliceObservableHandling()
        {
            var result = CompileBinding("VmArray.Select(item => item.ChildObject).Take(4).LastOrDefault().SomeString == 'abc'", new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("VmArray().map((item)=>ko.unwrap(item).ChildObject()).slice(0,4).at(-1).SomeString()==\"abc\"", result);
            //                                       not observable, because .map unwraps it ^^^^^^^
        }

        [TestMethod]
        [DataRow("Enumerable.Concat(LongArray, LongArray)", DisplayName = "Regular call of Enumerable.Concat")]
        [DataRow("LongArray.Concat(LongArray)", DisplayName = "Syntax sugar - extension method")]
        [DataRow("LongArray.ToImmutableArray().Concat(LongArray.ToImmutableArray())", DisplayName = "Immutable arrays")]
        public void JsTranslator_EnumerableConcat(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq"), new NamespaceImport("System.Collections.Immutable") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray().concat(LongArray())", result);
        }

        [TestMethod]
        [DataRow("Enumerable.Take(LongArray, 2)", DisplayName = "Regular call of Enumerable.Take")]
        [DataRow("LongArray.Take(2)", DisplayName = "Syntax sugar - extension method")]
        public void JsTranslator_EnumerableTake(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray().slice(0,2)", result);
        }

        [TestMethod]
        [DataRow("Enumerable.Skip(LongArray, 2)", DisplayName = "Regular call of Enumerable.Skip")]
        [DataRow("LongArray.Skip(2)", DisplayName = "Syntax sugar - extension method")]
        public void JsTranslator_EnumerableSkip(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray().slice(2)", result);
        }

        [TestMethod]
        public void JsTranslator_ListAdd()
        {
            var result = CompileBinding("LongList.Add(11)", new[] { typeof(TestViewModel) }, typeof(void), null);
            Assert.AreEqual("dotvvm.translations.array.add(LongList,11)", result);
        }

        [TestMethod]
        public void JsTranslator_ListAddOrUpdate()
        {
            var result = CompileBinding("LongList.AddOrUpdate(12345L, item => item == 12345, item => 54321L)", new[] { typeof(TestViewModel) }, typeof(void), new[] { new NamespaceImport("DotVVM.Framework.Binding.HelperNamespace") });
            Assert.AreEqual("dotvvm.translations.array.addOrUpdate(LongList,12345,(item)=>ko.unwrap(item)==12345,(item)=>54321)", result);
        }

        [TestMethod]
        public void JsTranslator_ListAddRange()
        {
            var result = CompileBinding("LongList.AddRange(LongArray)", new[] { typeof(TestViewModel) }, typeof(void), null);
            Assert.AreEqual("dotvvm.translations.array.addRange(LongList,LongArray())", result);
        }

        [TestMethod]
        [DataRow("!Enumerable.All(LongArray, (long item) => item > 0)", DisplayName = "Regular call of Enumerable.All")]
        [DataRow("!LongArray.All((long item) => item > 0)", DisplayName = "Syntax sugar - extension method")]
        [DataRow("!LongArray.ToImmutableArray().All((long item) => item > 0)", DisplayName = "Immutable array - extension method")]
        public void JsTranslator_EnumerableAll(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq"), new NamespaceImport("System.Collections.Immutable") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("!LongArray().every((item)=>ko.unwrap(item)>0)", result);
        }

        [TestMethod]
        [DataRow("!Enumerable.Any(LongArray, (long item) => item > 0)", DisplayName = "Regular call of Enumerable.Any")]
        [DataRow("!LongArray.Any((long item) => item > 0)", DisplayName = "Syntax sugar - extension method")]
        [DataRow("!LongArray.ToImmutableArray().Any((long item) => item > 0)", DisplayName = "Immutable array - extension method")]
        public void JsTranslator_EnumerableAny(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq"), new NamespaceImport("System.Collections.Immutable") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("!LongArray().some((item)=>ko.unwrap(item)>0)", result);
        }

        [TestMethod]
        public void JsTranslator_ListClear()
        {
            var result = CompileBinding("LongList.Clear()", new[] { typeof(TestViewModel) }, typeof(void), null);
            Assert.AreEqual("dotvvm.translations.array.clear(LongList)", result);
        }

        [TestMethod]
        public void JsTranslator_ICollectionClear()
        {
            var result = CompileBinding("TestViewModel2.Collection.Clear()", new[] { typeof(TestViewModel) }, typeof(void), null);
            Assert.AreEqual("dotvvm.translations.array.clear(TestViewModel2().Collection)", result);
        }

        [TestMethod]
        [DataRow("Enumerable.Empty<int>()")]
        [DataRow("Array.Empty<int>()")]
        [DataRow("ImmutableArray<int>.Empty")]
        [DataRow("ImmutableList<int>.Empty")]
        public void JsTranslator_EnumerableEmpty(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq"), new NamespaceImport("System.Collections.Immutable") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("[]", result);
        }


        [TestMethod]
        [DataRow("Enumerable.FirstOrDefault(LongArray) == 1", DisplayName = "Regular call of Enumerable.FirstOrDefault")]
        [DataRow("LongArray.FirstOrDefault() == 1", DisplayName = "Syntax sugar - extension method")]
        [DataRow("LongArray.ToImmutableArray().FirstOrDefault() == 1", DisplayName = "Immutable array - extension method")]
        public void JsTranslator_EnumerableFirstOrDefault(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq"), new NamespaceImport("System.Collections.Immutable") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray()[0]()==1", result);
        }

        [TestMethod]
        [DataRow("Enumerable.FirstOrDefault(LongArray, (long item) => item > 0)", DisplayName = "Regular call of Enumerable.FirstOrDefault")]
        [DataRow("LongArray.FirstOrDefault((long item) => item > 0)", DisplayName = "Syntax sugar - extension method")]
        [DataRow("LongArray.ToImmutableArray().FirstOrDefault((long item) => item > 0)", DisplayName = "Immutable array - extension method")]
        public void JsTranslator_EnumerableFirstOrDefaultParametrized(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq"), new NamespaceImport("System.Collections.Immutable") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray().find((item)=>ko.unwrap(item)>0)", result);
        }

        [TestMethod]
        public void JsTranslator_EnumerableFirstOrDefaultObservableHandling()
        {
            var withUnwraps = CompileBinding("TestJsTransations.GetTestObjectWithObservables().ObjectArray.FirstOrDefault(o => o.Enum == 'Value1').Int == 12", new[] { new NamespaceImport("System.Linq"), new NamespaceImport(typeof(TestJsTransations).Namespace) }, new[] { typeof(TestViewModel) });

            Assert.AreEqual("testPlainObject.ObjectArray().find((o)=>ko.unwrap(o).Enum()==\"Value1\")().Int()==12", withUnwraps);
        }

        [TestMethod]
        public void JsTranslator_EnumerableFirstOrDefaultObservableHandling2()
        {
            var withoutUnwraps = CompileBinding("TestJsTransations.GetTestPlainObject().ObjectArray.OrderBy(a => a.Enum).LastOrDefault().Int == 12", new[] { new NamespaceImport("System.Linq"), new NamespaceImport(typeof(TestJsTransations).Namespace) }, new[] { typeof(TestViewModel) });

            Assert.AreEqual("dotvvm.translations.array.orderBy(testPlainObject.ObjectArray,(a)=>ko.unwrap(a).Enum(),\"ObApbFFA2mUjzIJF\").at(-1).Int==12", withoutUnwraps);
        }

        [TestMethod]
        public void JsTranslator_ListInsert()
        {
            var result = CompileBinding("LongList.Insert(1, 12345)", new[] { typeof(TestViewModel) }, typeof(void), null);
            Assert.AreEqual("dotvvm.translations.array.insert(LongList,1,12345)", result);
        }

        [TestMethod]
        public void JsTranslator_ListInsertRange()
        {
            var result = CompileBinding("LongList.InsertRange(1, LongArray)", new[] { typeof(TestViewModel) }, typeof(void), null);
            Assert.AreEqual("dotvvm.translations.array.insertRange(LongList,1,LongArray())", result);
        }

        [TestMethod]
        [DataRow("String", "\"Hello\"", "\"Hello\"", DisplayName = "Contains call with string.")]
        [DataRow("", "1", "1", DisplayName = "Contains call with int.")]
        [DataRow("Enum", "TestEnum.A", "\"A\"", DisplayName = "Contains call with enum.")]
        [DataRow("Enum", "EnumProperty", "EnumProperty()", DisplayName = "Contains call with enum property.")]
        public void JsTranslator_ListContains(string listPrefix, string bindingValue, string jsValue)
        {
            var result = CompileBinding($"{listPrefix}List.Contains({bindingValue})", new[] { typeof(TestViewModel) }, typeof(bool), null);
            Assert.AreEqual($"dotvvm.translations.array.contains({listPrefix}List(),{jsValue})", result);
        }

        [TestMethod]
        public void JsTranslator_ICollectionContains()
        {
            var result = CompileBinding($"ReadOnlyCollection.Contains(12)", new[] { typeof(TestViewModel) });
            Assert.AreEqual($"dotvvm.translations.array.contains(ReadOnlyCollection(),12)", result);
        }

        [TestMethod]
        [DataRow("Enumerable.LastOrDefault(LongArray)", DisplayName = "Regular call of Enumerable.LastOrDefault")]
        [DataRow("LongArray.LastOrDefault()", DisplayName = "Syntax sugar - extension method")]
        [DataRow("LongArray.ToImmutableArray().LastOrDefault()", DisplayName = "Immutable array - extension method")]
        public void JsTranslator_EnumerableLastOrDefault(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq"), new NamespaceImport("System.Collections.Immutable") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray().at(-1)", result);
        }

        [TestMethod]
        [DataRow("Enumerable.LastOrDefault(LongArray, (long item) => item > 0)", DisplayName = "Regular call of Enumerable.LastOrDefault")]
        [DataRow("LongArray.LastOrDefault((long item) => item > 0)", DisplayName = "Syntax sugar - extension method")]
        public void JsTranslator_EnumerableLastOrDefaultParametrized(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq"), new NamespaceImport("System.Collections.Immutable") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray().findLast((item)=>ko.unwrap(item)>0)", result);
        }

        [TestMethod]
        public void JsTranslator_EnumerableLastOrDefaultObservable()
        {
            var result = CompileBinding("LongArray.LastOrDefault()==1", new[] { new NamespaceImport("System.Linq"), new NamespaceImport("System.Collections.Immutable") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray().at(-1)()==1", result);
        }

        [TestMethod]
        [DataRow("Enumerable.Distinct(VmArray)", DisplayName = "Regular call of Enumerable.Distinct")]
        [DataRow("VmArray.Distinct()", DisplayName = "Syntax sugar - extension method")]
        [ExpectedException(typeof(DotvvmCompilationException))]
        public void JsTranslator_EnumerableDistinct_NonPrimitiveTypesThrows(string binding)
        {
            CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
        }

        [TestMethod]
        [DataRow("Enumerable.Max(Int32Array)", "Int32Array", false, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(Int64Array)", "Int64Array", false, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(SingleArray)", "SingleArray",false, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(DoubleArray)", "DoubleArray", false, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(DecimalArray)", "DecimalArray", false, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(NullableInt32Array)", "NullableInt32Array", true, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(NullableInt64Array)", "NullableInt64Array", true, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(NullableDecimalArray)", "NullableDecimalArray", true, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(NullableSingleArray)", "NullableSingleArray", true, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(NullableDoubleArray)", "NullableDoubleArray", true, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Int32Array.Max()", "Int32Array", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("Int64Array.Max()", "Int64Array", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("SingleArray.Max()", "SingleArray", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("DoubleArray.Max()", "DoubleArray", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("DecimalArray.Max()", "DecimalArray", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableInt32Array.Max()", "NullableInt32Array", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableInt64Array.Max()", "NullableInt64Array", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableDecimalArray.Max()", "NullableDecimalArray", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableSingleArray.Max()", "NullableSingleArray", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableDoubleArray.Max()", "NullableDoubleArray", true, DisplayName = "Syntax sugar - extension method")]
        public void JsTranslator_EnumerableMax(string binding, string property, bool nullable)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestArraysViewModel) });
            Assert.AreEqual($"dotvvm.translations.array.max({property}(),(arg)=>ko.unwrap(arg))", result);
        }

        [TestMethod]
        [DataRow("Enumerable.Max(Int32Array, (int item) => -item)", "Int32Array", false, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(Int64Array, (long item) => -item)", "Int64Array", false, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(SingleArray, (float item) => -item)", "SingleArray", false, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(DoubleArray, (double item) => -item)", "DoubleArray", false, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(DecimalArray, (decimal item) => -item)", "DecimalArray", false, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(NullableInt32Array, item => -item)", "NullableInt32Array", true, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(NullableInt64Array, item => -item)", "NullableInt64Array", true, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(NullableDecimalArray, item => -item)", "NullableDecimalArray", true, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(NullableSingleArray, item => -item)", "NullableSingleArray", true, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Enumerable.Max(NullableDoubleArray, item => -item)", "NullableDoubleArray", true, DisplayName = "Regular call of Enumerable.Max")]
        [DataRow("Int32Array.Max(item => -item)", "Int32Array", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("Int64Array.Max(item => -item)", "Int64Array", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("SingleArray.Max(item => -item)", "SingleArray", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("DoubleArray.Max(item => -item)", "DoubleArray", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("DecimalArray.Max(item => -item)", "DecimalArray", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableInt32Array.Max(item => -item)", "NullableInt32Array", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableInt64Array.Max(item => -item)", "NullableInt64Array", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableDecimalArray.Max(item => -item)", "NullableDecimalArray", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableSingleArray.Max(item => -item)", "NullableSingleArray", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableDoubleArray.Max(item => -item)", "NullableDoubleArray", true, DisplayName = "Syntax sugar - extension method")]
        public void JsTranslator_EnumerableMax_WithSelector(string binding, string property, bool nullable)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestArraysViewModel) });
            Assert.AreEqual($"dotvvm.translations.array.max({property}(),(item)=>-ko.unwrap(item))", result);
        }

        [TestMethod]
        [DataRow("Enumerable.Min(Int32Array)", "Int32Array", false, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(Int64Array)", "Int64Array", false, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(SingleArray)", "SingleArray", false, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(DoubleArray)", "DoubleArray", false, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(DecimalArray)", "DecimalArray", false, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(NullableInt32Array)", "NullableInt32Array", true, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(NullableInt64Array)", "NullableInt64Array", true, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(NullableDecimalArray)", "NullableDecimalArray", true, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(NullableSingleArray)", "NullableSingleArray", true, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(NullableDoubleArray)", "NullableDoubleArray", true, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Int32Array.Min()", "Int32Array", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("Int64Array.Min()", "Int64Array", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("SingleArray.Min()", "SingleArray", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("DoubleArray.Min()", "DoubleArray", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("DecimalArray.Min()", "DecimalArray", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableInt32Array.Min()", "NullableInt32Array", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableInt64Array.Min()", "NullableInt64Array", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableDecimalArray.Min()", "NullableDecimalArray", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableSingleArray.Min()", "NullableSingleArray", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableDoubleArray.Min()", "NullableDoubleArray", true, DisplayName = "Syntax sugar - extension method")]
        public void JsTranslator_EnumerableMin(string binding, string property, bool nullable)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestArraysViewModel) });
            Assert.AreEqual($"dotvvm.translations.array.min({property}(),(arg)=>ko.unwrap(arg))", result);
        }

        [TestMethod]
        [DataRow("Enumerable.Min(Int32Array, item => -item)", "Int32Array", false, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(Int64Array, item => -item)", "Int64Array", false, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(SingleArray, item => -item)", "SingleArray", false, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(DoubleArray, item => -item)", "DoubleArray", false, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(DecimalArray, item => -item)", "DecimalArray", false, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(NullableInt32Array, item => -item)", "NullableInt32Array", true, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(NullableInt64Array, item => -item)", "NullableInt64Array", true, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(NullableDecimalArray, item => -item)", "NullableDecimalArray", true, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(NullableSingleArray, item => -item)", "NullableSingleArray", true, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Enumerable.Min(NullableDoubleArray, item => -item)", "NullableDoubleArray", true, DisplayName = "Regular call of Enumerable.Min")]
        [DataRow("Int32Array.Min(item => -item)", "Int32Array", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("Int64Array.Min(item => -item)", "Int64Array", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("SingleArray.Min(item => -item)", "SingleArray", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("DoubleArray.Min(item => -item)", "DoubleArray", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("DecimalArray.Min(item => -item)", "DecimalArray", false, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableInt32Array.Min(item => -item)", "NullableInt32Array", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableInt64Array.Min(item => -item)", "NullableInt64Array", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableDecimalArray.Min(item => -item)", "NullableDecimalArray", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableSingleArray.Min(item => -item)", "NullableSingleArray", true, DisplayName = "Syntax sugar - extension method")]
        [DataRow("NullableDoubleArray.Min(item => -item)", "NullableDoubleArray", true, DisplayName = "Syntax sugar - extension method")]
        public void JsTranslator_EnumerableMin_WithSelector(string binding, string property, bool nullable)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestArraysViewModel) });
            Assert.AreEqual($"dotvvm.translations.array.min({property}(),(item)=>-ko.unwrap(item))", result);
        }

        [DataRow("Int32Array")]
        [DataRow("NullableInt32Array")]
        [DataRow("Int64Array")]
        [DataRow("NullableInt64Array")]
        [DataRow("SingleArray")]
        [DataRow("NullableSingleArray")]
        [DataRow("DoubleArray")]
        [DataRow("NullableDoubleArray")]
        [DataRow("DecimalArray")]
        [DataRow("NullableDecimalArray")]
        [DataTestMethod]
        public void JsTranslator_EnumerableSum(string property)
        {
            var result = CompileBinding($"{property}.Sum()", new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestArraysViewModel) });
            XAssert.Equal($"{property}().reduce((acc,x)=>acc+(ko.unwrap(x)??0),0)", result);
            var resultSelector = CompileBinding($"{property}.Sum(x => -x)", new[] { new NamespaceImport("System.Linq"), new NamespaceImport("Math") }, new[] { typeof(TestArraysViewModel) });
            XAssert.Equal($"{property}().map((x)=>-ko.unwrap(x)).reduce((acc,x)=>acc+(x??0),0)", resultSelector);
        }

        [TestMethod]
        public void JsTranslator_EnumerableSum_Selector()
        {
            var result = CompileBinding($"ObjectArray.Sum(x => x.Int)", new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestArraysViewModel) });
            XAssert.Equal($"ObjectArray().map((x)=>ko.unwrap(x).Int()).reduce((acc,x)=>acc+(x??0),0)", result);
        }

        [TestMethod]
        [DataRow("Enumerable.OrderBy(ObjectArray, (TestComparisonType item) => item.Int)", "Int", typeof(int), DisplayName = "Regular call of Enumerable.OrderBy")]
        [DataRow("Enumerable.OrderBy(ObjectArray, (TestComparisonType item) => item.Bool)", "Bool", typeof(bool), DisplayName = "Regular call of Enumerable.OrderBy")]
        [DataRow("Enumerable.OrderBy(ObjectArray, (TestComparisonType item) => item.String)", "String", typeof(string), DisplayName = "Regular call of Enumerable.OrderBy")]
        [DataRow("Enumerable.OrderBy(ObjectArray, (TestComparisonType item) => item.Enum)", "Enum", typeof(TestComparisonType.TestEnum), DisplayName = "Regular call of Enumerable.OrderBy")]
        [DataRow("ObjectArray.OrderBy((TestComparisonType item) => item.Int)", "Int", typeof(int), DisplayName = "Syntax sugar - extension method")]
        [DataRow("ObjectArray.OrderBy((TestComparisonType item) => item.Bool)", "Bool", typeof(bool), DisplayName = "Syntax sugar - extension method")]
        [DataRow("ObjectArray.OrderBy((TestComparisonType item) => item.String)", "String", typeof(string), DisplayName = "Syntax sugar - extension method")]
        [DataRow("ObjectArray.OrderBy((TestComparisonType item) => item.Enum)", "Enum", typeof(TestComparisonType.TestEnum), DisplayName = "Syntax sugar - extension method")]
        public void JsTranslator_EnumerableOrderBy(string binding, string key, Type comparedType)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestArraysViewModel) });
            var typeHash = (comparedType.IsEnum) ? $"\"{comparedType.GetTypeHash()}\"" : "null";
            Assert.AreEqual($"dotvvm.translations.array.orderBy(ObjectArray(),(item)=>ko.unwrap(item).{key}(),{typeHash})", result);
        }

        [TestMethod]
        [DataRow("Enumerable.OrderBy(ObjectArray, (TestComparisonType item) => item.Obj)")]
        [DataRow("ObjectArray.OrderBy((TestComparisonType item) => item.Obj)")]
        [ExpectedException(typeof(DotvvmCompilationException))]
        public void JsTranslator_EnumerableOrderBy_NonPrimitiveTypesThrows(string binding)
        {
            CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestArraysViewModel) });
        }

        [TestMethod]
        [DataRow("Enumerable.OrderByDescending(ObjectArray, (TestComparisonType item) => item.Int)", "Int", typeof(int), DisplayName = "Regular call of Enumerable.OrderByDescending")]
        [DataRow("Enumerable.OrderByDescending(ObjectArray, (TestComparisonType item) => item.Bool)", "Bool", typeof(bool), DisplayName = "Regular call of Enumerable.OrderByDescending")]
        [DataRow("Enumerable.OrderByDescending(ObjectArray, (TestComparisonType item) => item.String)", "String", typeof(string), DisplayName = "Regular call of Enumerable.OrderByDescending")]
        [DataRow("Enumerable.OrderByDescending(ObjectArray, (TestComparisonType item) => item.Enum)", "Enum", typeof(TestComparisonType.TestEnum), DisplayName = "Regular call of Enumerable.OrderByDescending")]
        [DataRow("ObjectArray.OrderByDescending((TestComparisonType item) => item.Int)", "Int", typeof(int), DisplayName = "Syntax sugar - extension method")]
        [DataRow("ObjectArray.OrderByDescending((TestComparisonType item) => item.Bool)", "Bool", typeof(bool), DisplayName = "Syntax sugar - extension method")]
        [DataRow("ObjectArray.OrderByDescending((TestComparisonType item) => item.String)", "String", typeof(string), DisplayName = "Syntax sugar - extension method")]
        [DataRow("ObjectArray.OrderByDescending((TestComparisonType item) => item.Enum)", "Enum", typeof(TestComparisonType.TestEnum), DisplayName = "Syntax sugar - extension method")]
        public void JsTranslator_EnumerableOrderByDescending(string binding, string key, Type comparedType)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestArraysViewModel) });
            var typeHash = (comparedType.IsEnum) ? $"\"{comparedType.GetTypeHash()}\"" : "null";
            Assert.AreEqual($"dotvvm.translations.array.orderByDesc(ObjectArray(),(item)=>ko.unwrap(item).{key}(),{typeHash})", result);
        }

        [TestMethod]
        [DataRow("Enumerable.OrderByDescending(ObjectArray, (TestComparisonType item) => item.Obj)")]
        [DataRow("ObjectArray.OrderByDescending((TestComparisonType item) => item.Obj)")]
        [ExpectedException(typeof(DotvvmCompilationException))]
        public void JsTranslator_EnumerableOrderByDescending_NonPrimitiveTypesThrows(string binding)
        {
            CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestArraysViewModel) });
        }

        [TestMethod]
        public void JsTranslator_ListRemoveAt()
        {
            var result = CompileBinding("LongList.RemoveAt(1)", new[] { typeof(TestViewModel) }, typeof(void), null);
            Assert.AreEqual("dotvvm.translations.array.removeAt(LongList,1)", result);
        }
        [TestMethod]
        public void JsTranslator_IListRemoveAt()
        {
            var result = CompileBinding("TestViewModel2.Collection.RemoveAt(1)", new[] { typeof(TestViewModel) }, typeof(void), null);
            Assert.AreEqual("dotvvm.translations.array.removeAt(TestViewModel2().Collection,1)", result);
        }

        [TestMethod]
        public void JsTranslator_ListRemoveFirst()
        {
            var result = CompileBinding("LongList.RemoveFirst((long item) => item == 2)", new[] { typeof(TestViewModel) }, typeof(void), new[] { new NamespaceImport("DotVVM.Framework.Binding.HelperNamespace") });
            Assert.AreEqual("dotvvm.translations.array.removeFirst(LongList,(item)=>ko.unwrap(item)==2)", result);
        }

        [TestMethod]
        public void JsTranslator_ListRemoveLast()
        {
            var result = CompileBinding("LongList.RemoveLast((long item) => item == 2)", new[] { typeof(TestViewModel) }, typeof(void), new[] { new NamespaceImport("DotVVM.Framework.Binding.HelperNamespace") });
            Assert.AreEqual("dotvvm.translations.array.removeLast(LongList,(item)=>ko.unwrap(item)==2)", result);
        }

        [TestMethod]
        public void JsTranslator_ListRemoveRange()
        {
            var result = CompileBinding("LongList.RemoveRange(1, 5)", new[] { typeof(TestViewModel) }, typeof(void), null);
            Assert.AreEqual("dotvvm.translations.array.removeRange(LongList,1,5)", result);
        }

        [TestMethod]
        public void JsTranslator_ListReverse()
        {
            var result = CompileBinding("LongList.Reverse()", new[] { typeof(TestViewModel) }, typeof(void), null);
            Assert.AreEqual("dotvvm.translations.array.reverse(LongList)", result);
        }

        [TestMethod]
        [DataRow("Math.Abs(IntProp)", "Math.abs(IntProp())")]
        [DataRow("Math.Abs(DoubleProp)", "Math.abs(DoubleProp())")]
        [DataRow("Math.Acos(DoubleProp)", "Math.acos(DoubleProp())")]
        [DataRow("Math.Asin(DoubleProp)", "Math.asin(DoubleProp())")]
        [DataRow("Math.Atan(DoubleProp)", "Math.atan(DoubleProp())")]
        [DataRow("Math.Atan2(DoubleProp, 15)", "Math.atan2(DoubleProp(),15)")]
        [DataRow("Math.Ceiling(DoubleProp)", "Math.ceil(DoubleProp())")]
        [DataRow("Math.Cos(DoubleProp)", "Math.cos(DoubleProp())")]
        [DataRow("Math.Cosh(DoubleProp)", "Math.cosh(DoubleProp())")]
        [DataRow("Math.Exp(DoubleProp)", "Math.exp(DoubleProp())")]
        [DataRow("Math.Floor(DoubleProp)", "Math.floor(DoubleProp())")]
        [DataRow("Math.Log(DoubleProp)", "Math.log(DoubleProp())")]
        [DataRow("Math.Log10(DoubleProp)", "Math.log10(DoubleProp())")]
        [DataRow("Math.Max(IntProp, DoubleProp)", "Math.max(IntProp(),DoubleProp())")]
        [DataRow("Math.Min(IntProp, DoubleProp)", "Math.min(IntProp(),DoubleProp())")]
        [DataRow("Math.Pow(IntProp, 3)", "Math.pow(IntProp(),3)")]
        [DataRow("Math.Round(DoubleProp)", "Math.round(DoubleProp())")]
        [DataRow("Math.Round(DoubleProp, 2)", "DoubleProp().toFixed(2)")]
        [DataRow("Math.Sign(IntProp)", "Math.sign(IntProp())")]
        [DataRow("Math.Sign(DoubleProp)", "Math.sign(DoubleProp())")]
        [DataRow("Math.Sqrt(DoubleProp)", "Math.sqrt(DoubleProp())")]
        [DataRow("Math.Tan(DoubleProp)", "Math.tan(DoubleProp())")]
        [DataRow("Math.Tanh(DoubleProp)", "Math.tanh(DoubleProp())")]
        [DataRow("Math.Truncate(DoubleProp)", "Math.trunc(DoubleProp())")]
        public void JsTranslator_MathMethods(string binding, string expected)
        {
            var result = CompileBinding(binding, new[] { typeof(TestViewModel) });
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow("Convert.ToBoolean(IntProp)", "window.Boolean(IntProp())")]
        [DataRow("Convert.ToBoolean(DoubleProp)", "window.Boolean(DoubleProp())")]
        [DataRow("Convert.ToDecimal(DoubleProp)", "DoubleProp")]
        [DataRow("Convert.ToInt32(DoubleProp)", "Math.round(DoubleProp())")]
        [DataRow("Convert.ToByte(DoubleProp)", "Math.round(DoubleProp())")]
        [DataRow("Convert.ToDouble(IntProp)", "IntProp")]
        [DataRow("Convert.ToDouble(DecimalProp)", "DecimalProp")]
        public void JsTranslator_ConvertNumeric(string binding, string expected)
        {
            var result = CompileBinding(binding, new[] { typeof(TestViewModel) });
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow("StringProp.Split(\"str\")", "str", "None")]
        [DataRow("StringProp.Split('c', StringSplitOptions.None)", "c", "None")]
        [DataRow("StringProp.Split('c', StringSplitOptions.RemoveEmptyEntries)", "c", "RemoveEmptyEntries")]
        public void JsTranslator_StringSplit_WithOptions(string binding, string delimiter, string options)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("DotVVM.Framework.Binding.HelperNamespace") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual($"dotvvm.translations.string.split(StringProp(),\"{delimiter}\",\"{options}\")", result);
        }

        [TestMethod]
        [DataRow("StringProp.Split('c', 'b')", "[\"c\",\"b\"]")]
        [DataRow("StringProp.Split('c', 'b', 'a')", "[\"c\",\"b\",\"a\"]")]
        public void JsTranslator_StringSplit_ArrayDelimiters_NoOptions(string binding, string delimiters)
        {
            var result = CompileBinding(binding, new[] { typeof(TestViewModel) });
            Assert.AreEqual($"StringProp().split({delimiters})", result);
        }

        [TestMethod]
        [DataRow("string.Join('c', StringArray)", "c")]
        [DataRow("string.Join(\"str\", StringArray)", "str")]
        public void JsTranslator_StringArrayJoin(string binding, string delimiter)
        {
            var result = CompileBinding(binding, new[] { typeof(TestViewModel) });
            Assert.AreEqual($"dotvvm.translations.string.join(StringArray(),\"{delimiter}\")", result);
        }

        [TestMethod]
        [DataRow("string.Join('c', StringArray.Where((string item) => item.Length > 2))", "c")]
        [DataRow("string.Join(\"str\", StringArray.Where((string item) => item.Length > 2))", "str")]
        public void JsTranslator_StringEnumerableJoin(string binding, string delimiter)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual($"dotvvm.translations.string.join(StringArray().filter((item)=>ko.unwrap(item).length>2),\"{delimiter}\")", result);
        }

        [TestMethod]
        [DataRow("StringProp.Replace('c', 'a')", "c", "a")]
        [DataRow("StringProp.Replace(\"str\", \"rts\")", "str", "rts")]
        public void JsTranslator_StringReplace(string binding, string original, string replacement)
        {
            var result = CompileBinding(binding, new[] { typeof(TestViewModel) });
            Assert.AreEqual($"StringProp().split(\"{original}\").join(\"{replacement}\")", result);
        }

        [TestMethod]
        [DataRow("DateTime.Year", "getFullYear", false)]
        [DataRow("DateTime.Month", "getMonth", true)]
        [DataRow("DateTime.Day", "getDate", false)]
        [DataRow("DateTime.Hour", "getHours", false)]
        [DataRow("DateTime.Minute", "getMinutes", false)]
        [DataRow("DateTime.Second", "getSeconds", false)]
        [DataRow("DateTime.Millisecond", "getMilliseconds", false)]
        public void JsTranslator_DateTime_Property_Getters(string binding, string jsFunction, bool increment = false)
        {
            var result = CompileBinding(binding, new[] { typeof(TestViewModel) });
            Assert.AreEqual($"dotvvm.serialization.parseDate(DateTime()).{jsFunction}(){(increment ? "+1" : string.Empty)}", result);
        }

        [TestMethod]
        [DataRow("DateTime.Now", "dotvvm.serialization.serializeDate(new Date(),false)")]
        [DataRow("DateTime.UtcNow", "dotvvm.serialization.serializeDate(new Date(),true)")]
        [DataRow("DateTime.Today", "dotvvm.serialization.serializeDate(new Date(),false).substring(0,10)+\"T00:00:00.000\"")]
        public void JsTranslator_DateTime_Now(string binding, string expected)
        {
            var result = CompileBinding(binding);
            Assert.AreEqual(expected, result);
        }


        [TestMethod]
        [DataRow("DateOnly.ToString()", "")]
        [DataRow("DateOnly.ToString('D')", "\"D\"")]
        public void JsTranslator_DateOnly_ToString(string binding, string args)
        {
            var result = CompileBinding(binding, new[] { typeof(TestViewModel) });
            Assert.AreEqual($"dotvvm.globalize.bindingDateOnlyToString(DateOnly{((args.Length > 0) ? $",{args}" : string.Empty)})", result);
        }

        [TestMethod]
        [DataRow("NullableDateOnly.ToString()", "")]
        public void JsTranslator_NullableDateOnly_ToString(string binding, string args)
        {
            var result = CompileBinding(binding, new[] { typeof(TestViewModel) });
            Assert.AreEqual($"dotvvm.globalize.bindingDateOnlyToString(NullableDateOnly{((args.Length > 0) ? $",{args}" : string.Empty)})", result);
        }

        [DataTestMethod]
        [DataRow("DateOnly.Year", "getFullYear()")]
        [DataRow("DateOnly.Month", "getMonth()+1")]
        [DataRow("DateOnly.Day", "getDate()")]
        public void JsTranslator_DateOnly_Property_Getters(string binding, string jsFunction)
        {
            var result = CompileBinding(binding, new[] { typeof(TestViewModel) });
            Assert.AreEqual($"dotvvm.serialization.parseDateOnly(DateOnly()).{jsFunction}", result);
        }

        [TestMethod]
        [DataRow("TimeOnly.ToString()", "")]
        [DataRow("TimeOnly.ToString('T')", "\"T\"")]
        public void JsTranslator_TimeOnly_ToString(string binding, string args)
        {
            var result = CompileBinding(binding, new[] { typeof(TestViewModel) });
            Assert.AreEqual($"dotvvm.globalize.bindingTimeOnlyToString(TimeOnly{((args.Length > 0) ? $",{args}" : string.Empty)})", result);
        }

        [TestMethod]
        [DataRow("NullableTimeOnly.ToString()", "")]
        public void JsTranslator_NullableTimeOnly_ToString(string binding, string args)
        {
            var result = CompileBinding(binding, new[] { typeof(TestViewModel) });
            Assert.AreEqual($"dotvvm.globalize.bindingTimeOnlyToString(NullableTimeOnly{((args.Length > 0) ? $",{args}" : string.Empty)})", result);
        }

        [DataTestMethod]
        [DataRow("TimeOnly.Hour", "getHours()")]
        [DataRow("TimeOnly.Minute", "getMinutes()")]
        [DataRow("TimeOnly.Second", "getSeconds()")]
        [DataRow("TimeOnly.Millisecond", "getMilliseconds()")]
        public void JsTranslator_TimeOnly_Property_Getters(string binding, string jsFunction)
        {
            var result = CompileBinding(binding, new[] { typeof(TestViewModel) });
            Assert.AreEqual($"dotvvm.serialization.parseTimeOnly(TimeOnly()).{jsFunction}", result);
        }

        [TestMethod]
        public void JsTranslator_WebUtility_UrlEncode()
        {
            var result = CompileBinding("WebUtility.UrlEncode(\"Hello World!\")", new[] { new NamespaceImport("System.Net") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual($"encodeURIComponent(\"Hello World!\")", result);
        }

        [TestMethod]
        public void JsTranslator_WebUtility_UrlDecode()
        {
            var result = CompileBinding("WebUtility.UrlDecode(\"Hello%20World!\")", new[] { new NamespaceImport("System.Net") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual($"decodeURIComponent(\"Hello%20World!\")", result);
        }

        [TestMethod]
        public void JavascriptCompilation_GuidToString()
        {
            var result = CompileBinding("GuidProp != Guid.Empty ? GuidProp.ToString() : ''", typeof(TestViewModel));
            Assert.AreEqual("GuidProp()!=\"00000000-0000-0000-0000-000000000000\"?GuidProp:\"\"", result);
        }

        [TestMethod]
        [DataRow("_collection.IsEven", "$index()%2==0")]
        [DataRow("_this._collection.IsEven", "$index()%2==0")]
        [DataRow("_root._index", "$index()")]
        [DataRow("_index", "$index()")]
        [DataRow("_root._page.EvaluatingOnClient", "true")]
        [DataRow("_page.EvaluatingOnClient", "true")]
        public void StaticCommandCompilation_Parameters(string expr, string expectedResult)
        {
            var result = CompileBinding(expr, new [] { typeof(TestViewModel) });
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        [DataRow("_index", "$parentContext.$parentContext.$index()")]
        [DataRow("_parent2._index", "$parentContext.$parentContext.$index()")]
        [DataRow("_root._index", "$parentContext.$parentContext.$index()")]
        [DataRow("_collection.IsEven", "$parentContext.$parentContext.$index()%2==0")]
        public void StaticCommandCompilation_ParametersInHierarchy(string expr, string expectedResult)
        {
            var result = CompileBinding(expr, new [] { typeof(TestViewModel), typeof(object), typeof(string) });
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        [DataRow("_this._index", "IndexProperty")]
        [DataRow("_root._index", "IndexProperty")]
        [DataRow("_index", "IndexProperty")]
        [DataRow("_collection.Index", "$index()")]
        public void StaticCommandCompilation_ParameterPropertyConflict(string expr, string expectedResult)
        {
            var result = CompileBinding(expr, new [] { typeof(TestExtensionParameterConflictViewModel) });
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ParameterPropertyNotExists()
        {
            // _index exists on parent, but not on _this
            var e = Assert.ThrowsException<Exception>(() =>
                CompileBinding("_this._index", new [] { typeof(object), typeof(string) }));
            Assert.AreEqual("Could not find instance member _index on type string.", e.Message);
        }

        [TestMethod]
        public void StaticCommandCompilation_MultipleExplicitIndexParameters()
        {
            var dc1 = DataContextStack.Create(
                typeof(TestViewModel),
                extensionParameters: new [] {
                    new CurrentCollectionIndexExtensionParameter()
                });
            var dc2 = DataContextStack.Create(
                typeof(int),
                parent: dc1,
                extensionParameters: new [] {
                    new CurrentCollectionIndexExtensionParameter()
                });
            var result = bindingHelper.ValueBindingToJs("_index + _this._index + _parent._index + _root._index", dc2);
            Assert.AreEqual("$index() + $index() + $parentContext.$index() + $parentContext.$index()", result);
        }
        [TestMethod]
        public void StaticCommandCompilation_ExplicitIndexParameterInThis()
        {
            var result = CompileBinding("_this._index", new [] { typeof(TestViewModel) });
            Assert.AreEqual("$index()", result);
        }

        [TestMethod]
        public void JavascriptCompilation_Variable()
        {
            var result = CompileBinding("var a = 1; var b = 2; var c = 3; a + b + c", typeof(TestViewModel));
            Assert.AreEqual("(()=>{let a=1;let b=2;let c=3;return a+b+c;})()", result);
        }

        [TestMethod]
        public void JavascriptCompilation_Variable_Nested()
        {
            var result = CompileBinding("var a = 1; var b = (var a = 5; a + 1); a + b", typeof(TestViewModel));
            Assert.AreEqual("(()=>{let a0=1;let a=5;let b=a+1;return a0+b;})()", result);
        }

        [TestMethod]
        public void JavascriptCompilation_Variable_Property()
        {
            var result = CompileBinding("var a = _this.StringProp; var b = _this.StringProp2; StringProp2 = a + b", typeof(TestViewModel));
            Assert.AreEqual("(()=>{let a=StringProp();let b=StringProp2();return StringProp2(a+b).StringProp2;})()", result);
        }

        [TestMethod]
        public void JavascriptCompilation_Variable_VM()
        {
            var result = CompileBinding("var a = _parent; var b = _this.StringProp2; StringProp2 = a + b", new [] { typeof(string), typeof(TestViewModel) });
            Assert.AreEqual("(()=>{let a=$parent;let b=StringProp2();return StringProp2(a+b).StringProp2;})()", result);
        }

        [TestMethod]
        public void JavascriptCompilation_AssignAndUse()
        {
            var result = CompileBinding("StringProp2 = (_this.StringProp = _this.StringProp2 = 'lol') + 'hmm'", typeof(TestViewModel));
            Assert.AreEqual("StringProp2((StringProp(StringProp2(\"lol\").StringProp2()).StringProp()??\"\")+\"hmm\").StringProp2", result);
        }

        [TestMethod]
        public void JavascriptCompilation_AssignAndUseObject()
        {
            var result = CompileBinding("StringProp2 = (_this.TestViewModel2B = _this.TestViewModel2 = _this.VmArray[3]).SomeString", typeof(TestViewModel));
            Assert.AreEqual("StringProp2(dotvvm.serialization.deserialize(dotvvm.serialization.deserialize(VmArray()[3](),TestViewModel2,true)(),TestViewModel2B,true)().SomeString()).StringProp2", result);
        }

        [TestMethod, Ignore] // ignored because https://github.com/dotnet/corefx/issues/33074
        public void JavascriptCompilation_AssignAndUseObjectArray()
        {
            var result = CompileBinding("StringProp2 = (_this.VmArray[1] = (_this.VmArray[0] = _this.VmArray[3])).SomeString", typeof(TestViewModel));
            Assert.AreEqual("function(abc){return abc+\"def\";}", result);
        }

        [TestMethod]
        public void JavascriptCompilation_AssignmentExpectsObservable()
        {
            var result = CompileBinding("_api.RefreshOnChange(StringProp = StringProp2, StringProp + StringProp2)", typeof(TestViewModel));
            Assert.AreEqual("dotvvm.api.refreshOn(StringProp(StringProp2()).StringProp,ko.pureComputed(()=>(StringProp()??\"\")+(StringProp2()??\"\")))", result);
        }

        [TestMethod]
        public void JavascriptCompilation_ApiRefreshOn()
        {
            var result = CompileBinding("_api.RefreshOnChange('here would be the API invocation', StringProp + StringProp2)", typeof(TestViewModel));
            Assert.AreEqual("dotvvm.api.refreshOn(\"here would be the API invocation\",ko.pureComputed(()=>(StringProp()??\"\")+(StringProp2()??\"\")))", result);
        }

        [TestMethod]
        public void JavascriptCompilation_MarkupControlProperty()
        {
            _ = TestMarkupControl.SomePropertyProperty;
            var dataContext = bindingHelper.CreateDataContext(new [] { typeof(object) }, markupControl: typeof(TestMarkupControl));
            var result = bindingHelper.ValueBindingToJs("_control.SomeProperty + 'aa'", dataContext, niceMode: false);
            Assert.AreEqual("($control.SomeProperty()??\"\")+\"aa\"", result);
        }
        [TestMethod]
        public void JavascriptCompilation_MarkupControlProperty_NotDotvvmProperty()
        {
            // Using non-DotvvmProperty is JS binding is an error, the property doesn't exist client-side
            var dataContext = bindingHelper.CreateDataContext(new [] { typeof(object) }, markupControl: typeof(TestMarkupControl));
            // resource bindings should be fine, only value bindings don't support it
            var resourceResult = bindingHelper.ExecuteBinding<string>("_control.NonUsableProperty", new [] { new object() }, markupControl: new TestMarkupControl() { NonUsableProperty = "xxyyaa" });
            Assert.AreEqual("xxyyaa", resourceResult);
            var exception = XAssert.ThrowsAny<Exception>(() => bindingHelper.ValueBindingToJs("_control.NonUsableProperty", dataContext, niceMode: false));
            XAssert.Contains("Control property NonUsableProperty is not a registered DotvvmProperty and cannot be used client-side", exception.Message);
        }

        [DataTestMethod]
        [DataRow("StringProp.ToUpper()", "StringProp().toLocaleUpperCase()")]
        [DataRow("StringProp.ToLower()", "StringProp().toLocaleLowerCase()")]
        [DataRow("StringProp.ToUpperInvariant()", "StringProp().toUpperCase()")]
        [DataRow("StringProp.ToLowerInvariant()", "StringProp().toLowerCase()")]
        [DataRow("StringProp.IndexOf('test')", "StringProp().indexOf(\"test\")")]
        [DataRow("StringProp.IndexOf('test',StringComparison.InvariantCultureIgnoreCase)",
            "dotvvm.translations.string.indexOf(StringProp(),0,\"test\",\"InvariantCultureIgnoreCase\")")]
        [DataRow("StringProp.IndexOf('test',1)", "StringProp().indexOf(\"test\",1)")]
        [DataRow("StringProp.IndexOf('test',1,StringComparison.InvariantCultureIgnoreCase)",
            "dotvvm.translations.string.indexOf(StringProp(),1,\"test\",\"InvariantCultureIgnoreCase\")")]
        [DataRow("StringProp.LastIndexOf('test')", "StringProp().lastIndexOf(\"test\")")]
        [DataRow("StringProp.LastIndexOf('test',StringComparison.InvariantCultureIgnoreCase)",
            "dotvvm.translations.string.lastIndexOf(StringProp(),0,\"test\",\"InvariantCultureIgnoreCase\")")]
        [DataRow("StringProp.LastIndexOf('test',2)", "StringProp().lastIndexOf(\"test\",2)")]
        [DataRow("StringProp.LastIndexOf('test',2,StringComparison.InvariantCultureIgnoreCase)",
            "dotvvm.translations.string.lastIndexOf(StringProp(),2,\"test\",\"InvariantCultureIgnoreCase\")")]
        [DataRow("StringProp.Contains('test')", "StringProp().includes(\"test\")")]
        [DataRow("StringProp.Contains('test',StringComparison.InvariantCultureIgnoreCase)",
            "dotvvm.translations.string.contains(StringProp(),\"test\",\"InvariantCultureIgnoreCase\")")]
        [DataRow("StringProp.StartsWith('test')", "StringProp().startsWith(\"test\")")]
        [DataRow("StringProp.StartsWith('test',StringComparison.InvariantCultureIgnoreCase)",
            "dotvvm.translations.string.startsWith(StringProp(),\"test\",\"InvariantCultureIgnoreCase\")")]
        [DataRow("StringProp.EndsWith('test')", "StringProp().endsWith(\"test\")")]
        [DataRow("StringProp.EndsWith('test',StringComparison.InvariantCultureIgnoreCase)",
            "dotvvm.translations.string.endsWith(StringProp(),\"test\",\"InvariantCultureIgnoreCase\")")]
        [DataRow("StringProp.Trim()", "StringProp().trim()")]
        [DataRow("StringProp.PadLeft(1)", "StringProp().padStart(1)")]
        [DataRow("StringProp.PadRight(2)", "StringProp().padEnd(2)")]
        [DataRow("StringProp.PadLeft(1,'#')", "StringProp().padStart(1,\"#\")")]
        [DataRow("StringProp.PadRight(2,'#')", "StringProp().padEnd(2,\"#\")")]
        [DataRow("string.IsNullOrEmpty(StringProp)", "!(StringProp()?.length>0)")]
        [DataRow("string.IsNullOrWhiteSpace(StringProp)", "!(StringProp()?.trim().length>0)")]
#if DotNetCore
        [DataRow("StringProp.Trim('0')", "dotvvm.translations.string.trimEnd(dotvvm.translations.string.trimStart(StringProp(),\"0\"),\"0\")")]
        [DataRow("StringProp.TrimStart()", "StringProp().trimStart()")]
        [DataRow("StringProp.TrimStart('0')", "dotvvm.translations.string.trimStart(StringProp(),\"0\")")]
        [DataRow("StringProp.TrimEnd()", "StringProp().trimEnd()")]
        [DataRow("StringProp.TrimEnd('0')", "dotvvm.translations.string.trimEnd(StringProp(),\"0\")")]
#endif
#if !DotNetCore
        [DataRow("StringProp.Trim('0')", "dotvvm.translations.string.trimEnd(dotvvm.translations.string.trimStart(StringProp(),[\"0\"][0]),[\"0\"][0])")]
        [DataRow("StringProp.TrimStart()", "dotvvm.translations.string.trimStart(StringProp(),[][0])")]
        [DataRow("StringProp.TrimStart('0')", "dotvvm.translations.string.trimStart(StringProp(),[\"0\"][0])")]
        [DataRow("StringProp.TrimEnd()", "dotvvm.translations.string.trimEnd(StringProp(),[][0])")]
        [DataRow("StringProp.TrimEnd('0')", "dotvvm.translations.string.trimEnd(StringProp(),[\"0\"][0])")]
#endif
        public void JavascriptCompilation_StringFunctions(string input, string expected)
        {
            var result = CompileBinding(input, new[] { new NamespaceImport("DotVVM.Framework.Binding.HelperNamespace") }, typeof(TestViewModel));
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void JavascriptCompilation_CustomPrimitiveToString()
        {
            var result = CompileBinding("VehicleNumber.ToString()", typeof(TestViewModel));
            Assert.AreEqual("VehicleNumber", result);
            // nullable .Value unwrap
            result = CompileBinding("VehicleNumber.Value.ToString()", typeof(TestViewModel));
            Assert.AreEqual("VehicleNumber", result);
        }

        [TestMethod]
        public void JavascriptCompilation_CustomPrimitiveProperty_Disallowed()
        {
            var e = Assert.ThrowsException<NotSupportedException>(() => CompileBinding("VehicleNumber.Value.Value", typeof(TestViewModel)));
            XAssert.Contains("Cannot translate property Value on custom primitive type DotVVM.Framework.Tests.Binding.VehicleNumber", e.Message);
        }

        [TestMethod]
        public void JavascriptCompilation_CustomPrimitiveParse()
        {
            var result = CompileBinding("VehicleNumber == DotVVM.Framework.Tests.Binding.VehicleNumber.Parse('123')", typeof(TestViewModel));
            Assert.AreEqual("VehicleNumber()==\"123\"", result);
        }

        [DataTestMethod]
        [DataRow("BindAttribute", "bind_attribute")]
        [DataRow("SystemTextJson", "system_text_json")]
        [DataRow("NewtonsoftJson", "newtonsoft_json")]
        public void JavascriptCompilation_PropertyRenaming(string csharpName, string jsName)
        {
            var result = CompileBinding($"{csharpName} == 0", typeof(TestPropertyRenamingViewModel));
            Assert.AreEqual($"{jsName}()==0", result);
        }

        [TestMethod]
        public void JavascriptCompilation_ParametrizedCode_ParentContexts()
        {
            // root: TestViewModel
            // parent: TestViewModel2 + _index + _collection
            // this: TestViewModel3 + _index
            var context0 = DataContextStack.Create(typeof(TestViewModel));
            var context1 = DataContextStack.Create(typeof(TestViewModel2), parent: context0, extensionParameters: [ new CurrentCollectionIndexExtensionParameter(), new BindingCollectionInfoExtensionParameter("_collection") ]);
            var context2 = DataContextStack.Create(typeof(TestViewModel3), parent: context1, extensionParameters: [ new CurrentCollectionIndexExtensionParameter() ]);

            var result = bindingHelper.ValueBindingToParametrizedCode("_root.IntProp + _parent._index + _parent._collection.Index + _parent.MyProperty + _index + SomeString.Length", context2);

            Assert.AreEqual("$parents[1].IntProp() + $parentContext.$index() + $parentContext.$index() + $parent.MyProperty() + $index() + SomeString().length", result.ToDefaultString());

            // assign `context` and `vm` variables
            var formatted2 = result.ToString(o =>
                o == JavascriptTranslator.KnockoutContextParameter ? new ParametrizedCode("context", OperatorPrecedence.Max) :
                o == JavascriptTranslator.KnockoutViewModelParameter ? new ParametrizedCode("vm", OperatorPrecedence.Max) :
                default(CodeParameterAssignment)
            );
            Console.WriteLine(formatted2);

            var symbolicParameters = result.Parameters.ToArray();
            Assert.AreEqual(6, symbolicParameters.Length);
            Assert.AreEqual(2, XAssert.IsAssignableFrom<JavascriptTranslator.ViewModelSymbolicParameter>(symbolicParameters[0].Parameter).ParentIndex);
            Assert.AreEqual(JavascriptTranslator.KnockoutContextParameter, symbolicParameters[1].Parameter); // JavascriptTranslator.ParentKnockoutContextParameter would also work
            Assert.AreEqual(JavascriptTranslator.KnockoutContextParameter, symbolicParameters[2].Parameter); // JavascriptTranslator.ParentKnockoutContextParameter would also work
            Assert.AreEqual(JavascriptTranslator.ParentKnockoutViewModelParameter, symbolicParameters[3].Parameter);
            Assert.AreEqual(JavascriptTranslator.KnockoutContextParameter, symbolicParameters[4].Parameter);
            Assert.AreEqual(JavascriptTranslator.KnockoutViewModelParameter, symbolicParameters[5].Parameter);

            Assert.AreEqual("context.$parents[1].IntProp() + context.$parentContext.$index() + context.$parentContext.$index() + context.$parent.MyProperty() + context.$index() + vm.SomeString().length", formatted2);
        }

        public class TestMarkupControl: DotvvmMarkupControl
        {
            public string SomeProperty
            {
                get { return (string)GetValue(SomePropertyProperty); }
                set { SetValue(SomePropertyProperty, value); }
            }
            public static readonly DotvvmProperty SomePropertyProperty =
                DotvvmProperty.Register<string, TestMarkupControl>(nameof(SomeProperty));

            public string NonUsableProperty { get; set; }
        }


        public class TestPropertyRenamingViewModel
        {
            [Bind(Name = "bind_attribute")]
            public int BindAttribute { get; }
            [JsonPropertyName("system_text_json")]
            public int SystemTextJson { get; }
            [Newtonsoft.Json.JsonProperty("newtonsoft_json")]
            public int NewtonsoftJson { get; }
        }
    }

    public static class TestJsTransations
    {
        // returns object which is not wrapped in ko.observables
        public static TestArraysViewModel GetTestPlainObject() => new TestArraysViewModel();
        // returns object which is wrapped in ko.observables
        public static TestArraysViewModel GetTestObjectWithObservables() => new TestArraysViewModel();
    }

    public class TestExtensionParameterConflictViewModel
    {
        [Bind(Name = "IndexProperty")]
        public int _index { get; }
    }

    public class TestApiClient
    {
        public string GetString() => "";
        public string PostDateToString(DateTime date) => date.ToShortDateString();
        public DateTime GetCurrentTime(string name) => DateTime.UtcNow;
    }

    public class TestArraysViewModel
    {
        public int[] Int32Array { get; set; } = new[] { 1, 2, 3 };
        public long[] Int64Array { get; set; } = new[] { 1L, 2L, 3L };
        public decimal[] DecimalArray { get; set; } = new[] { 1m, 2m, 3m };
        public float[] SingleArray { get; set; } = new[] { 1f, 2f, 3f };
        public double[] DoubleArray { get; set; } = new[] { 1d, 2d, 3d };
        public int?[] NullableInt32Array { get; set; } = new int?[] { 1, 2, 3 };
        public long?[] NullableInt64Array { get; set; } = new long?[] { 1, 2, 3 };
        public decimal?[] NullableDecimalArray { get; set; } = new decimal?[] { 1, 2, 3 };
        public float?[] NullableSingleArray { get; set; } = new float?[] { 1, 2, 3 };
        public double?[] NullableDoubleArray { get; set; } = new double?[] { 1, 2, 3 };
        public TestComparisonType[] ObjectArray { get; set; } = new[] { new TestComparisonType() };
    }

    public class TestComparisonType
    {
        public enum TestEnum
        {
            Value1,
            Value2
        }

        public int Int { get; set; }
        public object Obj { get; set; }
        public bool Bool { get; set; }
        public TestEnum Enum { get; set; }
        public string String { get; set; }
    }
}
