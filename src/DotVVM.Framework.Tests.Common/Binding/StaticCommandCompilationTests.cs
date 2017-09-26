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
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class StaticCommandCompilationTests
    {

        /// Gets translation of the specified binding expression if it would be passed in static command
        /// For better readability, the returned code does not include null checks
        public string CompileBinding(string expression, params Type[] contexts) => CompileBinding(expression, contexts, expectedType: typeof(object));
        public string CompileBinding(string expression, Type[] contexts, Type expectedType)
        {
            var configuration = DotvvmTestHelper.CreateConfiguration();
            configuration.RegisterApiClient(typeof(TestApiClient), "http://server/api", "./apiscript.js", "_api");
            configuration.Markup.ImportedNamespaces.Add(new NamespaceImport("DotVVM.Framework.Tests.Binding"));

            var context = DataContextStack.Create(contexts.FirstOrDefault() ?? typeof(object), extensionParameters: new BindingExtensionParameter[]{
                new BindingPageInfoExtensionParameter(),
                }.Concat(configuration.Markup.DefaultExtensionParameters).ToArray());
            for (int i = 1; i < contexts.Length; i++)
            {
                context = DataContextStack.Create(contexts[i], context);
            }
            var parser = new BindingExpressionBuilder();
            var expressionTree = TypeConversion.ImplicitConversion(parser.Parse(expression, context, BindingParserOptions.Create<ValueBindingExpression>()), expectedType, true, true);
            var jsExpression =
                configuration.ServiceLocator.GetService<StaticCommandBindingCompiler>().CompileToJavascript(context, expressionTree);
            return KnockoutHelper.GenerateClientPostbackScript("", new Literal(),
                BindingPropertyResolvers.FormatJavascript(jsExpression, nullChecks: false),
                new PostbackScriptOptions(
                    allowPostbackHandlers: false,
                    returnValue: null
                ));
        }

        [TestMethod]
        public void StaticCommandCompilation_SimpleCommand()
        {
            var result = CompileBinding("StaticCommands.GetLength(StringProp)", typeof(TestViewModel));
            Assert.AreEqual("(function(a,b,c){return (dotvvm.staticCommandPostback('root',a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIiwiQUE9PSJd\",[b.$data.StringProp()],function(r_0){c.resolve(r_0);}),c);}(this,ko.contextFor(this),new DotvvmPromise()))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_AssignedCommand()
        {
            var result = CompileBinding("StringProp = StaticCommands.GetLength(StringProp).ToString()", typeof(TestViewModel));
            Assert.AreEqual("(function(a,b,c){return (dotvvm.staticCommandPostback('root',a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIiwiQUE9PSJd\",[b.$data.StringProp()],function(r_0){c.resolve(b.$data.StringProp(dotvvm.globalize.bindingNumberToString(r_0)()).StringProp());}),c);}(this,ko.contextFor(this),new DotvvmPromise()))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_JsOnlyCommand()
        {
            var result = CompileBinding("StringProp = StringProp.Length.ToString()", typeof(TestViewModel));
            Assert.AreEqual("(function(a,b){return (b.resolve(a.$data.StringProp(dotvvm.globalize.bindingNumberToString(a.$data.StringProp().length)()).StringProp()),b);}(ko.contextFor(this),new DotvvmPromise()))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ChainedCommands()
        {
            var result = CompileBinding("StringProp = StaticCommands.GetLength(StaticCommands.GetLength(StringProp).ToString()).ToString()", typeof(TestViewModel));
            Assert.AreEqual("(function(a,b,c){return (dotvvm.staticCommandPostback('root',a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIiwiQUE9PSJd\",[b.$data.StringProp()],function(r_0){dotvvm.staticCommandPostback('root',a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIiwiQUE9PSJd\",[dotvvm.globalize.bindingNumberToString(r_0)()],function(r_1){c.resolve(b.$data.StringProp(dotvvm.globalize.bindingNumberToString(r_1)()).StringProp());});}),c);}(this,ko.contextFor(this),new DotvvmPromise()))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ChainedCommandsWithSemicolon()
        {
            var result = CompileBinding("StringProp = StaticCommands.GetLength(StringProp).ToString(); StringProp = StaticCommands.GetLength(StringProp).ToString()", typeof(TestViewModel));
            Assert.AreEqual("(function(a,c,d,b){return (dotvvm.staticCommandPostback('root',a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIiwiQUE9PSJd\",[c.$data.StringProp()],function(r_0){(b=c.$data.StringProp(dotvvm.globalize.bindingNumberToString(r_0)()).StringProp(),dotvvm.staticCommandPostback('root',a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIiwiQUE9PSJd\",[c.$data.StringProp()],function(r_1){d.resolve((b,c.$data.StringProp(dotvvm.globalize.bindingNumberToString(r_1)()).StringProp()));}));}),d);}(this,ko.contextFor(this),new DotvvmPromise()))", result);
        }
    }

    public static class StaticCommands
    {
        [AllowStaticCommand]
        public static int GetLength(string str) => str.Length;
    }
}