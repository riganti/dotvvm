using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.Configuration;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Binding.Properties;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class JavascriptCompilationTests
    {
        private DotvvmConfiguration configuration;
        private BindingCompilationService bindingService;

        [TestInitialize]
        public void Init()
        {
            this.configuration = DotvvmTestHelper.CreateConfiguration();
            configuration.RegisterApiClient(typeof(TestApiClient), "http://server/api", "./apiscript.js", "_testApi");
            this.bindingService = configuration.ServiceProvider.GetRequiredService<BindingCompilationService>();
        }
        public string CompileBinding(string expression, params Type[] contexts) => CompileBinding(expression, contexts, expectedType: typeof(object));
        public string CompileBinding(string expression, NamespaceImport[] imports, params Type[] contexts) => CompileBinding(expression, contexts, expectedType: typeof(object), imports);
        public string CompileBinding(string expression, Type[] contexts, Type expectedType, NamespaceImport[] imports = null)
        {
            var context = DataContextStack.Create(contexts.FirstOrDefault() ?? typeof(object), extensionParameters: new BindingExtensionParameter[]{
                new CurrentCollectionIndexExtensionParameter(),
                new BindingCollectionInfoExtensionParameter("_collection"),
                new BindingPageInfoExtensionParameter(),
                new BindingApiExtensionParameter(),
                }.Concat(configuration.Markup.DefaultExtensionParameters).ToArray());
            for (int i = 1; i < contexts.Length; i++)
            {
                context = DataContextStack.Create(contexts[i], context);
            }
            var parser = new BindingExpressionBuilder(configuration.ServiceProvider.GetRequiredService<CompiledAssemblyCache>(), configuration.ServiceProvider.GetRequiredService<ExtensionMethodsCache>());
            var parsedExpression = parser.ParseWithLambdaConversion(expression, context, BindingParserOptions.Create<ValueBindingExpression>(importNs: imports), expectedType);
            var expressionTree =
                TypeConversion.MagicLambdaConversion(parsedExpression, expectedType) ??
                TypeConversion.ImplicitConversion(parsedExpression, expectedType, true, true);
            var jsExpression = new JsParenthesizedExpression(configuration.ServiceProvider.GetRequiredService<JavascriptTranslator>().CompileToJavascript(expressionTree, context));
            jsExpression.AcceptVisitor(new KnockoutObservableHandlingVisitor(true));
            JsTemporaryVariableResolver.ResolveVariables(jsExpression);
            return JavascriptTranslator.FormatKnockoutScript(jsExpression.Expression);
        }

        public ValueBindingExpression CompileValueBinding(string expression, Type[] contexts, Type expectedType)
        {
            var context = DataContextStack.Create(contexts.FirstOrDefault() ?? typeof(object), extensionParameters: new BindingExtensionParameter[]{
                new BindingPageInfoExtensionParameter(),
                }.Concat(configuration.Markup.DefaultExtensionParameters).ToArray());
            for (int i = 1; i < contexts.Length; i++)
            {
                context = DataContextStack.Create(contexts[i], context);
            }

            var valueBinding = new ValueBindingExpression(bindingService, new object[] {
                context,
                new OriginalStringBindingProperty(expression),
                new BindingParserOptions(typeof(ValueBindingExpression)).AddImports(configuration.Markup.ImportedNamespaces),
                new ExpectedTypeBindingProperty(expectedType ?? typeof(object))
            });
            return valueBinding;
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
            return JavascriptTranslator.FormatKnockoutScript(jsExpression.Expression);
        }

        [TestMethod]
        public void JavascriptCompilation_EnumComparison()
        {
            var js = CompileBinding($"_this == 'Local'", typeof(DateTimeKind));
            Assert.AreEqual("$data==\"Local\"", js);
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
        public void JavascriptCompilation_UnwrappedObservables()
        {
            var js = CompileBinding("TestViewModel2.Collection[0].StringValue.Length + TestViewModel2.Collection[8].StringValue", new[] { typeof(TestViewModel) });
            Assert.AreEqual("TestViewModel2().Collection()[0]().StringValue().length+TestViewModel2().Collection()[8]().StringValue()", js);
        }

        [TestMethod]
        public void JavascriptCompilation_Parent()
        {
            var js = CompileBinding("_parent + _parent2 + _parent0 + _parent1 + _parent3", typeof(string), typeof(string), typeof(string), typeof(string), typeof(string))
                .Replace("(", "").Replace(")", "");
            Assert.AreEqual("$parent+$parents[1]+$data+$parent+$parents[2]", js);
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
            Assert.AreEqual("function(parameter,local){local=6+$data;return local;}", result);
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
            Assert.AreEqual("function(local){local=6+$data;return local;}()", result);
        }

        [TestMethod]
        public void JavascriptCompilation_Api_GetFunction()
        {
            var result = CompileBinding("_testApi.GetString()");
            Assert.AreEqual("dotvvm.api.invoke(dotvvm.api._testApi,\"getString\",function(){return [];},function(args){return [dotvvm.eventHub.get(\"DotVVM.Framework.Tests.Binding.TestApiClient/\")];},function(args){return [];},null,function(args){return \"\";})", result);
            var assignment = CompileBinding("StringProp = _testApi.GetString()", typeof(TestViewModel));
            Assert.AreEqual("StringProp(dotvvm.api.invoke(dotvvm.api._testApi,\"getString\",function(){return [];},function(args){return [dotvvm.eventHub.get(\"DotVVM.Framework.Tests.Binding.TestApiClient/\")];},function(args){return [];},null,function(args){return \"\";})()).StringProp", assignment);
        }

        [TestMethod]
        public void JavascriptCompilation_Api_GetDate()
        {
            var result = CompileBinding("_testApi.GetCurrentTime('test')");
            Assert.AreEqual("dotvvm.api.invoke(dotvvm.api._testApi,\"getCurrentTime\",function(){return [\"test\"];},function(args){return [dotvvm.eventHub.get(\"DotVVM.Framework.Tests.Binding.TestApiClient/\")];},function(args){return [];},null,function(args){return \"\";})", result);
            var assignment = CompileBinding("DateFrom = _testApi.GetCurrentTime('test')", typeof(TestViewModel));
            Assert.AreEqual("DateFrom(dotvvm.serialization.serializeDate(dotvvm.api.invoke(dotvvm.api._testApi,\"getCurrentTime\",function(){return [\"test\"];},function(args){return [dotvvm.eventHub.get(\"DotVVM.Framework.Tests.Binding.TestApiClient/\")];},function(args){return [];},null,function(args){return \"\";})(),false)).DateFrom", assignment);
        }

        [TestMethod]
        public void JavascriptCompilation_Api_DateParameter()
        {
            var result = CompileBinding("_testApi.PostDateToString(DateFrom.Value)", typeof(TestViewModel));
            Assert.IsTrue(result.StartsWith("dotvvm.api.invoke(dotvvm.api._testApi,\"postDateToString\",function(){return [dotvvm.globalize.parseDotvvmDate(DateFrom())];},function(args){return [];},function(args){return [\"DotVVM.Framework.Tests.Binding.TestApiClient/\"];},$element,function(args){return \""));
            Assert.IsTrue(result.EndsWith("\";})"));
        }

        [TestMethod]
        public void JavascriptCompilation_WrappedIdentifierExpression()
        {
            var result = CompileValueBinding("_this", new [] {typeof(TestViewModel) }, typeof(object));
            Assert.AreEqual("$data", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("$rawData", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("$rawData", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }

        [TestMethod]
        public void JavascriptCompilation_WrappedPropertyAccessExpression()
        {
            var result = CompileValueBinding("StringProp", new [] {typeof(TestViewModel) }, typeof(object));
            Assert.AreEqual("StringProp()", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("StringProp", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("StringProp", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }

        [TestMethod]
        public void JavascriptCompilation_WrappedNestedPropertyAccessExpression()
        {
            var result = CompileValueBinding("TestViewModel2.SomeString", new[] { typeof(TestViewModel) }, typeof(object));
            Assert.AreEqual("TestViewModel2()&&TestViewModel2().SomeString()", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("(TestViewModel2()||{}).SomeString", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("dotvvm.evaluator.wrapObservable(function(){return TestViewModel2()&&TestViewModel2().SomeString;})", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }

        [TestMethod]
        public void JavascriptCompilation_WrappedNestedListAccessExpression()
        {
            var result = CompileValueBinding("TestViewModel2.Collection", new[] { typeof(TestViewModel) }, typeof(object));
            Assert.AreEqual("TestViewModel2()&&TestViewModel2().Collection()", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("(TestViewModel2()||{}).Collection", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("dotvvm.evaluator.wrapObservable(function(){return TestViewModel2()&&TestViewModel2().Collection;},true)", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }

        [TestMethod]
        public void JavascriptCompilation_WrappedNegatedBooleanAccessExpression()
        {
            var result = CompileValueBinding("!Value", new[] { typeof(Something) }, typeof(object));
            Assert.AreEqual("!Value()", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("!Value()", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("ko.pureComputed(function(){return !Value();})", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }

        [TestMethod]
        public void JavascriptCompilation_WrappedExpression()
        {
            var result = CompileValueBinding("StringProp.Length + 43", new [] {typeof(TestViewModel) }, typeof(object));
            Assert.AreEqual("(StringProp()==null?null:StringProp().length)+43", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("(StringProp()==null?null:StringProp().length)+43", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("ko.pureComputed(function(){return (StringProp()==null?null:StringProp().length)+43;})", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }

        [TestMethod]
        public void JavascriptCompilation_FormatStringExpression()
        {
            var result = CompileValueBinding("LongProperty.ToString('0000')", new [] {typeof(TestViewModel) }, typeof(object));
            Assert.AreEqual("dotvvm.globalize.bindingNumberToString(LongProperty,\"0000\")()", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("dotvvm.globalize.bindingNumberToString(LongProperty,\"0000\")", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("dotvvm.globalize.bindingNumberToString(LongProperty,\"0000\")", FormatKnockoutScript(result.WrappedKnockoutExpression));
        }


        [TestMethod]
        public void JsTranslator_SimpleCSharpExpression()
        {
            Expression<Func<string, string>> expr = abc => abc + "def";
            var tree = configuration.ServiceProvider.GetRequiredService<JavascriptTranslator>().CompileToJavascript(expr, DataContextStack.Create(typeof(object)));
            Assert.AreEqual("function(abc){return abc+\"def\";}", tree.ToString());
        }

        [TestMethod]
        public void JsTranslator_StaticFieldInCSharpExpression()
        {
            Expression<Func<string, string>> expr = _ => string.Empty;
            var tree = configuration.ServiceProvider.GetRequiredService<JavascriptTranslator>().CompileToJavascript(expr, DataContextStack.Create(typeof(object)));
            Assert.AreEqual("function(_){return \"\";}", tree.ToString());
        }

        [TestMethod]
        public void JsTranslator_LambdaWithParameter()
        {
            var result = this.CompileBinding("_this + arg", new [] { typeof(string) }, typeof(Func<string, string>));
            Assert.AreEqual("function(arg){return $data+ko.unwrap(arg);}", result);
        }

        [TestMethod]
        public void JsTranslator_LambdaWithDelegateInvocation()
        {
            var result = this.CompileBinding("arg(12) + _this", new [] { typeof(string) }, typeof(Func<Func<int, string>, string>));
            Assert.AreEqual("function(arg){return ko.unwrap(arg)(12)+$data;}", result);
        }

        [TestMethod]
        public void JsTranslator_EnumToString()
        {
            var result = CompileBinding("EnumProperty.ToString()", typeof(TestViewModel));
            var resultImplicit = CompileBinding("EnumProperty", new [] { typeof(TestViewModel) }, typeof(string));

            Assert.AreEqual(result, resultImplicit);
            Assert.AreEqual("EnumProperty", result);
        }

        [TestMethod]
        public void JsTranslator_DataContextShift()
        {
            var result = CompileValueBinding("_this.StringProp", new [] { typeof(TestViewModel) }, typeof(string));
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
        public void JsTranslator_ArrayIndexer()
        {
            var result = CompileBinding("LongArray[1] == 3 && VmArray[0].MyProperty == 1 && VmArray.Length > 1", new [] { typeof(TestViewModel)});
            Assert.AreEqual("LongArray()[1]()==3&&(VmArray()[0]().MyProperty()==1&&VmArray().length>1)", result);
        }

        [TestMethod]
        [DataRow("Enumerable.Where(LongArray, (long item) => item % 2 == 0)", DisplayName = "Regular call of Enumerable.Where")]
        [DataRow("LongArray.Where((long item) => item % 2 == 0)", DisplayName = "Syntax sugar - extension method")]
        public void JsTranslator_EnumerableWhere(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray().filter(function(item){return ko.unwrap(item)%2==0;})", result);
        }

        [TestMethod]
        public void JsTranslator_NestedEnumerableMethods()
        {
            var result = CompileBinding("Enumerable.Where(Enumerable.Where(LongArray, (long item) => item % 2 == 0), (long item) => item % 3 == 0)",
                new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });

            Assert.AreEqual("LongArray().filter(function(item){return ko.unwrap(item)%2==0;}).filter(function(item){return ko.unwrap(item)%3==0;})", result);
        }

        [TestMethod]
        [DataRow("Enumerable.Select(LongArray, (long item) => -item)", DisplayName = "Regular call of Enumerable.Select")]
        [DataRow("LongArray.Select((long item) => -item)", DisplayName = "Syntax sugar - extension method")]
        public void JsTranslator_EnumerableSelect(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray().map(function(item){return -ko.unwrap(item);})", result);
        }

        [TestMethod]
        [DataRow("Enumerable.Concat(LongArray, LongArray)", DisplayName = "Regular call of Enumerable.Concat")]
        [DataRow("LongArray.Concat(LongArray)", DisplayName = "Syntax sugar - extension method")]
        public void JsTranslator_EnumerableConcat(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray().concat(LongArray())", result);
        }

        [TestMethod]
        [DataRow("Enumerable.Take(LongArray, 2)", DisplayName = "Regular call of Enumerable.Take")]
        [DataRow("LongArray.Take(2)", DisplayName = "Syntax sugar - extension method")]
        public void JsTraslator_EnumerableTake(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray().slice(0,2)", result);
        }

        [TestMethod]
        [DataRow("Enumerable.Skip(LongArray, 2)", DisplayName = "Regular call of Enumerable.Skip")]
        [DataRow("LongArray.Skip(2)", DisplayName = "Syntax sugar - extension method")]
        public void JsTraslator_EnumerableSkip(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("LongArray().slice(2)", result);
        }

        [TestMethod]
        [DataRow("Enumerable.OrderBy(LongArray, (long item) => item)", DisplayName = "Regular call of Enumerable.OrderBy")]
        [DataRow("LongArray.OrderBy((long item) => item)", DisplayName = "Syntax sugar - extension method")]
        public void JsTraslator_EnumerableOrderBy(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("dotvvm.orderBy(LongArray(),function(item){return ko.unwrap(item);})", result);
        }

        [TestMethod]
        [DataRow("Enumerable.OrderByDescending(LongArray, (long item) => item)", DisplayName = "Regular call of Enumerable.OrderByDescending")]
        [DataRow("LongArray.OrderByDescending((long item) => item)", DisplayName = "Syntax sugar - extension method")]
        public void JsTraslator_EnumerableOrderByDescending(string binding)
        {
            var result = CompileBinding(binding, new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) });
            Assert.AreEqual("dotvvm.orderByDesc(LongArray(),function(item){return ko.unwrap(item);})", result);
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
        [DataRow("Math.Round(DoubleProp, 2)", "Math.round(DoubleProp()).toFixed(2)")]
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
        public void JsTranslator_ValidMethod_UnsupportedTranslation()
        {
            Assert.ThrowsException<NotSupportedException>(() =>
                CompileBinding("Enumerable.Skip<long>(LongArray, 2)", new[] { new NamespaceImport("System.Linq") }, new[] { typeof(TestViewModel) }));
        }

        [TestMethod]
        public void JavascriptCompilation_GuidToString()
        {
            var result = CompileBinding("GuidProp != Guid.Empty ? GuidProp.ToString() : ''", typeof(TestViewModel));
            Assert.AreEqual("GuidProp()!=\"00000000-0000-0000-0000-000000000000\"?GuidProp:\"\"", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_IndexParameter()
        {
            var result = CompileBinding("_index", new [] { typeof(TestViewModel)});
            Assert.AreEqual("$index()", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_CollectionInfoParameter()
        {
            var result = CompileBinding("_collection.IsEven", typeof(TestViewModel));
            Assert.AreEqual("$index()%2==0", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_IndexParameterInParent()
        {
            var result = CompileBinding("_index", new [] { typeof(TestViewModel), typeof(object), typeof(string) });
            Assert.AreEqual("$parentContext.$parentContext.$index()", result);
        }

        [TestMethod]
        public void JavascriptCompilation_AssignAndUse()
        {
            var result = CompileBinding("StringProp2 = (_this.StringProp = _this.StringProp2 = 'lol') + 'hmm'", typeof(TestViewModel));
            Assert.AreEqual("StringProp2(StringProp(StringProp2(\"lol\").StringProp2()).StringProp()+\"hmm\").StringProp2", result);
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
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void JavascriptCompilation_AssignmentExpectsObservable()
        {
            var result = CompileBinding("_api.RefreshOnChange(StringProp = StringProp2, StringProp + StringProp2)", typeof(TestViewModel));
            Assert.AreEqual("dotvvm.api.refreshOn(StringProp(StringProp2()).StringProp,ko.pureComputed(function(){return StringProp()+StringProp2();}))", result);
        }

        [TestMethod]
        public void JavascriptCompilation_ApiRefreshOn()
        {
            var result = CompileBinding("_api.RefreshOnChange('here would be the API invocation', StringProp + StringProp2)", typeof(TestViewModel));
            Assert.AreEqual("dotvvm.api.refreshOn(\"here would be the API invocation\",ko.pureComputed(function(){return StringProp()+StringProp2();}))", result);
        }

        [DataTestMethod]
        [DataRow("StringProp.ToUpper()", "StringProp().toUpperCase()")]
        [DataRow("StringProp.ToLower()", "StringProp().toLowerCase()")]
        [DataRow("StringProp.IndexOf('test')", "StringProp().indexOf(\"test\")")]
        [DataRow("StringProp.IndexOf('test',1)", "StringProp().indexOf(\"test\",1)")]
        [DataRow("StringProp.LastIndexOf('test')", "StringProp().lastIndexOf(\"test\")")]
        [DataRow("StringProp.LastIndexOf('test',2)", "StringProp().lastIndexOf(\"test\",2)")]
        [DataRow("StringProp.Contains('test')", "StringProp().includes(\"test\")")]
        [DataRow("StringProp.StartsWith('test')", "StringProp().startsWith(\"test\")")]
        [DataRow("StringProp.EndsWith('test')", "StringProp().endsWith(\"test\")")]
        [DataRow("string.IsNullOrEmpty(StringProp)", "StringProp()==null||StringProp()===\"\"")]
        public void JavascriptCompilation_StringFunctions(string input, string expected)
        {
            var result = CompileBinding(input, typeof(TestViewModel));
            Assert.AreEqual(expected, result);
        }
    }

    public class TestApiClient
    {
        public string GetString() => "";
        public string PostDateToString(DateTime date) => date.ToShortDateString();
        public DateTime GetCurrentTime(string name) => DateTime.UtcNow;
    }
}
