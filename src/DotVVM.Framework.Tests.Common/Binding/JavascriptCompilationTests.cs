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
        public void INIT()
        {
            this.configuration = DotvvmTestHelper.CreateConfiguration();
            configuration.RegisterApiClient(typeof(TestApiClient), "http://server/api", "./apiscript.js", "_api");
            this.bindingService = configuration.ServiceProvider.GetRequiredService<BindingCompilationService>();
        }
        public string CompileBinding(string expression, params Type[] contexts) => CompileBinding(expression, contexts, expectedType: typeof(object));
        public string CompileBinding(string expression, Type[] contexts, Type expectedType)
        {
            var context = DataContextStack.Create(contexts.FirstOrDefault() ?? typeof(object), extensionParameters: new BindingExtensionParameter[]{
                new BindingPageInfoExtensionParameter(),
                }.Concat(configuration.Markup.DefaultExtensionParameters).ToArray());
            for (int i = 1; i < contexts.Length; i++)
            {
                context = DataContextStack.Create(contexts[i], context);
            }
            var parser = new BindingExpressionBuilder();
            var parsedExpression = parser.ParseWithLambdaConversion(expression, context, BindingParserOptions.Create<ValueBindingExpression>(), expectedType);
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
            var configuration = DotvvmTestHelper.CreateConfiguration();
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
            var result = CompileBinding("_api.GetString()");
            Assert.AreEqual("dotvvm.invokeApiFn(function(){return dotvvm.api._api.getString();},[dotvvm.eventHub.get(\"dotvvm.api._api\")],[])", result);
            var assignment = CompileBinding("StringProp = _api.GetString()", typeof(TestViewModel));
            Assert.AreEqual("StringProp(dotvvm.invokeApiFn(function(){return dotvvm.api._api.getString();},[dotvvm.eventHub.get(\"dotvvm.api._api\")],[])()).StringProp()", assignment);
        }

        [TestMethod]
        public void JavascriptCompilation_Api_GetDate()
        {
            var result = CompileBinding("_api.GetCurrentTime('test')");
            Assert.AreEqual("dotvvm.invokeApiFn(function(){return dotvvm.api._api.getCurrentTime(\"test\");},[dotvvm.eventHub.get(\"dotvvm.api._api\")],[])", result);
            var assignment = CompileBinding("DateFrom = _api.GetCurrentTime('test')", typeof(TestViewModel));
            Assert.AreEqual("DateFrom(dotvvm.serialization.serializeDate(dotvvm.invokeApiFn(function(){return dotvvm.api._api.getCurrentTime(\"test\");},[dotvvm.eventHub.get(\"dotvvm.api._api\")],[])())).DateFrom()", assignment);
        }

        [TestMethod]
        public void JavascriptCompilation_Api_DateParameter()
        {
            var result = CompileBinding("_api.PostDateToString(DateFrom.Value)", typeof(TestViewModel));
            Assert.AreEqual("dotvvm.invokeApiFn(function(){return dotvvm.api._api.postDateToString(dotvvm.globalize.parseDotvvmDate(DateFrom()));},[],[\"dotvvm.api._api\"])", result);
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
        public void JavascriptCompilation_WrappedExpression()
        {
            var result = CompileValueBinding("StringProp.Length + 43", new [] {typeof(TestViewModel) }, typeof(object));
            Assert.AreEqual("(StringProp()==null?null:StringProp().length)+43", FormatKnockoutScript(result.UnwrappedKnockoutExpression));
            Assert.AreEqual("(StringProp()==null?null:StringProp().length)+43", FormatKnockoutScript(result.KnockoutExpression));
            Assert.AreEqual("dotvvm.evaluator.wrapKnockoutExpression(function(){return (StringProp()==null?null:StringProp().length)+43;})", FormatKnockoutScript(result.WrappedKnockoutExpression));
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
    }


    public class TestApiClient
    {
        public string GetString() => "";
        public string PostDateToString(DateTime date) => date.ToShortDateString();
        public DateTime GetCurrentTime(string name) => DateTime.UtcNow;
    }
}
