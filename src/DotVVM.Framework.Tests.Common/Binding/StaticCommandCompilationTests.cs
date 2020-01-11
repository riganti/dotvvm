using DotVVM.Framework.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Runtime.Filters;
using System.Collections.Immutable;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class StaticCommandCompilationTests
    {
        /// Gets translation of the specified binding expression if it would be passed in static command
        /// For better readability, the returned code does not include null checks
        public string CompileBinding(string expression, params Type[] contexts) => CompileBinding(expression, contexts, expectedType: typeof(Command));
        public string CompileBinding(string expression, Type[] contexts, Type expectedType)
        {
            var configuration = DotvvmTestHelper.CreateConfiguration();
            configuration.RegisterApiClient(typeof(TestApiClient), "http://server/api", "./apiscript.js", "_api");
            configuration.Markup.ImportedNamespaces.Add(new NamespaceImport("DotVVM.Framework.Tests.Binding"));

            var context = DataContextStack.Create(contexts.FirstOrDefault() ?? typeof(object), extensionParameters: new BindingExtensionParameter[]{
                new CurrentCollectionIndexExtensionParameter(),
                new BindingPageInfoExtensionParameter(),
                new InjectedServiceExtensionParameter("injectedService", new ResolvedTypeDescriptor(typeof(TestService))),
                }.Concat(configuration.Markup.DefaultExtensionParameters).ToArray());
            for (int i = 1; i < contexts.Length; i++)
            {
                context = DataContextStack.Create(contexts[i], context);
            }

            var parser = new BindingExpressionBuilder();
            var expressionTree = parser.ParseWithLambdaConversion(expression, context, BindingParserOptions.Create<ValueBindingExpression>(), expectedType);
            var jsExpression =
                configuration.ServiceProvider.GetRequiredService<StaticCommandBindingCompiler>().CompileToJavascript(context, expressionTree);
            return KnockoutHelper.GenerateClientPostBackScript(
                "",
                new FakeCommandBinding(BindingPropertyResolvers.FormatJavascript(jsExpression, nullChecks: false), null),
                new Literal(),
                new PostbackScriptOptions(
                    allowPostbackHandlers: false,
                    returnValue: null,
                    commandArgs: CodeParameterAssignment.FromIdentifier("commandArguments")
                ));
        }

        [TestMethod]
        public void StaticCommandCompilation_SimpleCommand()
        {
            var result = CompileBinding("StaticCommands.GetLength(StringProp)", typeof(TestViewModel));
            Assert.AreEqual("(function(a,b){return new Promise(function(resolve,reject){dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIixbXSwiQUE9PSJd\",[b.$data.StringProp()],function(r_0){resolve(r_0);},reject);});}(this,ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_AssignedCommand()
        {
            var result = CompileBinding("StringProp = StaticCommands.GetLength(StringProp).ToString()", typeof(TestViewModel));
            Assert.AreEqual("(function(a,b){return new Promise(function(resolve,reject){dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIixbXSwiQUE9PSJd\",[b.$data.StringProp()],function(r_0){resolve(b.$data.StringProp(dotvvm.globalize.bindingNumberToString(r_0)()).StringProp());},reject);});}(this,ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_JsOnlyCommand()
        {
            var result = CompileBinding("StringProp = StringProp.Length.ToString()", typeof(TestViewModel));
            Assert.AreEqual("(function(a){return Promise.resolve(a.$data.StringProp(dotvvm.globalize.bindingNumberToString(a.$data.StringProp().length)()).StringProp());}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ChainedCommands()
        {
            var result = CompileBinding("StringProp = StaticCommands.GetLength(StaticCommands.GetLength(StringProp).ToString()).ToString()", typeof(TestViewModel));
            Assert.AreEqual("(function(a,b){return new Promise(function(resolve,reject){dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIixbXSwiQUE9PSJd\",[b.$data.StringProp()],function(r_0){dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIixbXSwiQUE9PSJd\",[dotvvm.globalize.bindingNumberToString(r_0)()],function(r_1){resolve(b.$data.StringProp(dotvvm.globalize.bindingNumberToString(r_1)()).StringProp());},reject);},reject);});}(this,ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ChainedCommandsWithSemicolon()
        {
            var result = CompileBinding("StringProp = StaticCommands.GetLength(StringProp).ToString(); StringProp = StaticCommands.GetLength(StringProp).ToString()", typeof(TestViewModel));
            Assert.AreEqual("(function(a,c,b){return new Promise(function(resolve,reject){dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIixbXSwiQUE9PSJd\",[c.$data.StringProp()],function(r_0){(b=c.$data.StringProp(dotvvm.globalize.bindingNumberToString(r_0)()).StringProp(),dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIixbXSwiQUE9PSJd\",[c.$data.StringProp()],function(r_1){resolve((b,c.$data.StringProp(dotvvm.globalize.bindingNumberToString(r_1)()).StringProp()));},reject));},reject);});}(this,ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_DateTimeResultAssignment()
        {
            var result = CompileBinding("DateFrom = StaticCommands.GetDate()", typeof(TestViewModel));
            Assert.AreEqual("(function(a,b){return new Promise(function(resolve,reject){dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0RGF0ZSIsW10sIiJd\",[],function(r_0){resolve(b.$data.DateFrom(dotvvm.serialization.serializeDate(r_0,false)).DateFrom());},reject);});}(this,ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_DateTimeAssignment()
        {
            var result = CompileBinding("DateFrom = DateTo", typeof(TestViewModel));
            Assert.AreEqual("(function(a){return Promise.resolve(a.$data.DateFrom(dotvvm.serialization.serializeDate(a.$data.DateTo(),false)).DateFrom());}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_CommandArgumentUsage()
        {
            var result = CompileBinding("StringProp = arg.ToString()", new [] { typeof(TestViewModel) }, typeof(Func<int, Task>));
            Assert.AreEqual("(function(a){return Promise.resolve(a.$data.StringProp(dotvvm.globalize.bindingNumberToString(commandArguments[0])()).StringProp());}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_PossibleAmniguousMatch()
        {
            var result = CompileBinding("SomeString = injectedService.Load(SomeString)", new[] { typeof(TestViewModel3) }, typeof(Func<string, string>));

            Assert.AreEqual("(function(a,b){return new Promise(function(resolve,reject){dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiTG9hZCIsW10sIkFRQT0iXQ==\",[b.$data.SomeString()],function(r_0){resolve(b.$data.SomeString(r_0).SomeString());},reject);});}(this,ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_IndexParameter()
        {
            var result = CompileBinding("IntProp = _index", new [] { typeof(TestViewModel) });
            Assert.AreEqual("(function(a){return Promise.resolve(a.$data.IntProp(a.$index()).IntProp());}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_IndexParameterInParent()
        {
            var result = CompileBinding("_parent2.IntProp = _index", new [] { typeof(TestViewModel), typeof(object), typeof(string) });
            Assert.AreEqual("(function(a){return Promise.resolve(a.$parents[1].IntProp(a.$parentContext.$parentContext.$index()).IntProp());}(ko.contextFor(this)))", result);
        }
    }

    public class FakeCommandBinding : ICommandBinding
    {
        private readonly ParametrizedCode commandJavascript;
        private readonly BindingDelegate bindingDelegate;

        public FakeCommandBinding(ParametrizedCode commandJavascript, BindingDelegate bindingDelegate)
        {
            this.commandJavascript = commandJavascript;
            this.bindingDelegate = bindingDelegate;
        }
        public ParametrizedCode CommandJavascript => commandJavascript ?? throw new NotImplementedException();

        public BindingDelegate BindingDelegate => bindingDelegate ?? throw new NotImplementedException();

        public ImmutableArray<IActionFilter> ActionFilters => ImmutableArray<IActionFilter>.Empty;

        public object GetProperty(Type type, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException)
        {
            if (errorMode == ErrorHandlingMode.ReturnNull)
                return null;
            else if (errorMode == ErrorHandlingMode.ReturnException)
                return new NotImplementedException();
            else throw new NotImplementedException();
        }
    }

    public static class StaticCommands
    {
        [AllowStaticCommand]
        public static int GetLength(string str) => str.Length;

        [AllowStaticCommand]
        public static DateTime GetDate() => DateTime.UtcNow;
    }

    public abstract class TestInnerService<TOutput> 
    {
        public abstract TOutput Load(string text);
        public abstract TOutput Load(string text1, string text2);
    }

    public class TestService : TestInnerService<string>
    {

        [AllowStaticCommand]
        public override string Load(string text) => null;
        [AllowStaticCommand]
        public override string Load(string text1, string text2) => null;
    }
}
