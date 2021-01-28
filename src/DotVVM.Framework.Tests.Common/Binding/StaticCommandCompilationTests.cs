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
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class StaticCommandCompilationTests
    {
        /// Gets translation of the specified binding expression if it would be passed in static command
        /// For better readability, the returned code does not include null checks
        public string CompileBinding(string expression, bool niceMode, params Type[] contexts) => CompileBinding(expression, niceMode, contexts, expectedType: typeof(Command));
        public string CompileBinding(string expression, bool niceMode, Type[] contexts, Type expectedType, Type currentMarkupControl = null)
        {
            var configuration = DotvvmTestHelper.CreateConfiguration();

            configuration.RegisterApiClient(typeof(TestApiClient), "http://server/api", "./apiscript.js", "_api");
            configuration.Markup.ImportedNamespaces.Add(new NamespaceImport("DotVVM.Framework.Tests.Binding"));
            configuration.Markup.ImportedNamespaces.Add(new NamespaceImport(typeof(JsMethodsExtensions).FullName, "_ext"));
            configuration.Markup.JavascriptTranslator.MethodCollection.AddMethodTranslator(
                                              typeof(JsMethodsExtensions),
                                             nameof(JsMethodsExtensions.Test),
                                             new GenericMethodCompiler((a) =>
                                             new JsIdentifierExpression("MethodExtensions")
                                                            .Member("test")
                                                            .Invoke(
                                                            a[1].WithAnnotation(ShouldBeObservableAnnotation.Instance),
                                                            a[2].WithAnnotation(ShouldBeObservableAnnotation.Instance))
                                                             .WithAnnotation(new ResultIsPromiseAnnotation(e => e))
                                                       ), 2, allowMultipleMethods: true);

            var parameters =
                new BindingExtensionParameter[]{
                    new CurrentCollectionIndexExtensionParameter(),
                    new BindingPageInfoExtensionParameter(),
                    new InjectedServiceExtensionParameter("injectedService", new ResolvedTypeDescriptor(typeof(TestService)))
                }
                .Concat(configuration.Markup.DefaultExtensionParameters);

            if (currentMarkupControl != null)
            {
                parameters = parameters.Append(new CurrentMarkupControlExtensionParameter(new ResolvedTypeDescriptor(currentMarkupControl)));
            }

            var context = DataContextStack.Create(
                contexts.FirstOrDefault() ?? typeof(object),
                extensionParameters: parameters.ToArray(),
                imports: configuration.Markup.ImportedNamespaces.ToImmutableList());

            for (int i = 1; i < contexts.Length; i++)
            {
                context = DataContextStack.Create(contexts[i], context);
            }

            var options = BindingParserOptions.Create<ValueBindingExpression>()
                .AddImports(configuration.Markup.ImportedNamespaces);

            var parser = new BindingExpressionBuilder(configuration.ServiceProvider.GetRequiredService<CompiledAssemblyCache>());
            var expressionTree = parser.ParseWithLambdaConversion(expression, context, options, expectedType);
            var jsExpression =
                configuration.ServiceProvider.GetRequiredService<StaticCommandBindingCompiler>().CompileToJavascript(context, expressionTree);
            return KnockoutHelper.GenerateClientPostBackExpression(
                "",
                new FakeCommandBinding(BindingPropertyResolvers.FormatJavascript(jsExpression, nullChecks: false, niceMode: niceMode), null),
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
            var result = CompileBinding("StaticCommands.GetLength(StringProp)", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("(function(a,b){return new Promise(function(resolve,reject){dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIixbXSwiQUE9PSJd\",[b.$data.StringProp()],options).then(function(r_0){resolve(r_0);},reject);});}(this,ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_AssignedCommand()
        {
            var result = CompileBinding("StringProp = StaticCommands.GetLength(StringProp).ToString()", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("(function(a,b){return new Promise(function(resolve,reject){dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIixbXSwiQUE9PSJd\",[b.$data.StringProp()],options).then(function(r_0){resolve(b.$data.StringProp(dotvvm.globalize.bindingNumberToString(r_0)()).StringProp());},reject);});}(this,ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_JsOnlyCommand()
        {
            var result = CompileBinding("StringProp = StringProp.Length.ToString()", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("(function(a){return Promise.resolve(a.$data.StringProp(dotvvm.globalize.bindingNumberToString(a.$data.StringProp().length)()).StringProp());}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ChainedCommands()
        {
            var result = CompileBinding("StringProp = StaticCommands.GetLength(StaticCommands.GetLength(StringProp).ToString()).ToString()", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("(function(a,b){return new Promise(function(resolve,reject){dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIixbXSwiQUE9PSJd\",[b.$data.StringProp()],options).then(function(r_0){dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIixbXSwiQUE9PSJd\",[dotvvm.globalize.bindingNumberToString(r_0)()],options).then(function(r_1){resolve(b.$data.StringProp(dotvvm.globalize.bindingNumberToString(r_1)()).StringProp());},reject);},reject);});}(this,ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ChainedCommandsWithSemicolon()
        {
            var result = CompileBinding("StringProp = StaticCommands.GetLength(StringProp).ToString(); StringProp = StaticCommands.GetLength(StringProp).ToString()", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("(function(a,c,b){return new Promise(function(resolve,reject){dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIixbXSwiQUE9PSJd\",[c.$data.StringProp()],options).then(function(r_0){(b=c.$data.StringProp(dotvvm.globalize.bindingNumberToString(r_0)()).StringProp(),dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0TGVuZ3RoIixbXSwiQUE9PSJd\",[c.$data.StringProp()],options).then(function(r_1){resolve((b,c.$data.StringProp(dotvvm.globalize.bindingNumberToString(r_1)()).StringProp()));},reject));},reject);});}(this,ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_DateTimeResultAssignment()
        {
            var result = CompileBinding("DateFrom = StaticCommands.GetDate()", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("(function(a,b){return new Promise(function(resolve,reject){dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuU3RhdGljQ29tbWFuZHMsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiR2V0RGF0ZSIsW10sIiJd\",[],options).then(function(r_0){resolve(b.$data.DateFrom(dotvvm.serialization.serializeDate(r_0,false)).DateFrom());},reject);});}(this,ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_DateTimeAssignment()
        {
            var result = CompileBinding("DateFrom = DateTo", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("(function(a){return Promise.resolve(a.$data.DateFrom(dotvvm.serialization.serializeDate(a.$data.DateTo(),false)).DateFrom());}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_CommandArgumentUsage()
        {
            var result = CompileBinding("StringProp = arg.ToString()", niceMode: false, new[] { typeof(TestViewModel) }, typeof(Func<int, Task>));
            Assert.AreEqual("(function(a){return Promise.resolve(a.$data.StringProp(dotvvm.globalize.bindingNumberToString(commandArguments[0])()).StringProp());}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_PossibleAmniguousMatch()
        {
            var result = CompileBinding("SomeString = injectedService.Load(SomeString)", niceMode: false, new[] { typeof(TestViewModel3) }, typeof(Func<string, string>));

            Assert.AreEqual("(function(a,b){return new Promise(function(resolve,reject){dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiTG9hZCIsW10sIkFRQT0iXQ==\",[b.$data.SomeString()],options).then(function(r_0){resolve(b.$data.SomeString(r_0).SomeString());},reject);});}(this,ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_IndexParameter()
        {
            var result = CompileBinding("IntProp = _index", niceMode: false, new[] { typeof(TestViewModel) });
            Assert.AreEqual("(function(a){return Promise.resolve(a.$data.IntProp(a.$index()).IntProp());}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_IndexParameterInParent()
        {
            var result = CompileBinding("_parent2.IntProp = _index", niceMode: false, new[] { typeof(TestViewModel), typeof(object), typeof(string) });
            Assert.AreEqual("(function(a){return Promise.resolve(a.$parents[1].IntProp(a.$parentContext.$parentContext.$index()).IntProp());}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ExpressionBetweenPostbacks_WithParameters()
        {
            var result = CompileBinding("StringProp = injectedService.Load(StringProp, StringProp); \"Test\"; StringProp = injectedService.Load(StringProp)", niceMode: false, new[] { typeof(TestViewModel) });
            Assert.AreEqual("(function(a,c,b){return new Promise(function(resolve,reject){dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiTG9hZCIsW10sIkFRQUEiXQ==\",[c.$data.StringProp(),c.$data.StringProp()],options).then(function(r_0){(b=(c.$data.StringProp(r_0).StringProp(),\"Test\"),dotvvm.staticCommandPostback(a,\"WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiTG9hZCIsW10sIkFRQT0iXQ==\",[c.$data.StringProp()],options).then(function(r_1){resolve((b,c.$data.StringProp(r_1).StringProp()));},reject));},reject);});}(this,ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ExpressionBetweenPostbacks_NoParametersLast()
        {
            var result = CompileBinding("StringProp = injectedService.Load(IntProp); \"Test\"; StringProp2 = injectedService.Load()", niceMode: true, new[] { typeof(TestViewModel) });
            var control = @"(function(a, b) {
	return new Promise(function(resolve, reject) {
		dotvvm.staticCommandPostback(a, ""WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiTG9hZCIsW10sIkFRQT0iXQ=="", [b.$data.IntProp()], options).then(function(r_0) {
			dotvvm.staticCommandPostback(a, ""WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiTG9hZCIsW10sIkFRPT0iXQ=="", [], options).then(function(r_1) {
				resolve((
					b.$data.StringProp(r_0).StringProp() ,
					""Test"" ,
					b.$data.StringProp2(r_1).StringProp2()
				));
			}, reject);
		}, reject);
	});
}(this, ko.contextFor(this)))";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ExpressionBetweenPostbacks_VoidTypeFirst()
        {
            var result = CompileBinding("injectedService.Save(IntProp); \"Test\"; StringProp = injectedService.Load()", niceMode: true, new[] { typeof(TestViewModel) });

            var control = @"
(function(a, b) {
	return new Promise(function(resolve, reject) {
		dotvvm.staticCommandPostback(a, ""WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiU2F2ZSIsW10sIkFRQT0iXQ=="", [b.$data.IntProp()], options).then(function(r_0) {
			dotvvm.staticCommandPostback(a, ""WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiTG9hZCIsW10sIkFRPT0iXQ=="", [], options).then(function(r_1) {
				resolve((
					r_0 ,
					""Test"" ,
					b.$data.StringProp(r_1).StringProp()
				));
			}, reject);
		}, reject);
	});
}(this, ko.contextFor(this)))";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ExpressionBetweenAsyncPostbacks_TaskTypeFirst()
        {
            var result = CompileBinding("injectedService.SaveAsync(IntProp); \"Test\"; StringProp = injectedService.LoadAsync().Result", niceMode: true, new[] { typeof(TestViewModel) });

            var control = @"(function(a, b) {
	return new Promise(function(resolve, reject) {
		dotvvm.staticCommandPostback(a, ""WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiU2F2ZUFzeW5jIixbXSwiQVFBPSJd"", [b.$data.IntProp()], options).then(function(r_0) {
			dotvvm.staticCommandPostback(a, ""WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiTG9hZEFzeW5jIixbXSwiQVE9PSJd"", [], options).then(function(r_1) {
				resolve((
					r_0 ,
					(
						""Test"" ,
						b.$data.StringProp(r_1).StringProp()
					)
				));
			}, reject);
		}, reject);
	});
}(this, ko.contextFor(this)))";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_DependentPostbacks_TaskTypeFirst()
        {
            var result = CompileBinding("StringProp = injectedService.Load(IntProp); StringProp = injectedService.Load(StringProp)", niceMode: true, new[] { typeof(TestViewModel) });

            var control = @"(function(a, c, b) {
	return new Promise(function(resolve, reject) {
		dotvvm.staticCommandPostback(a, ""WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiTG9hZCIsW10sIkFRQT0iXQ=="", [c.$data.IntProp()], options).then(function(r_0) {
			(
				b = c.$data.StringProp(r_0).StringProp() ,
				dotvvm.staticCommandPostback(a, ""WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiTG9hZCIsW10sIkFRQT0iXQ=="", [c.$data.StringProp()], options).then(function(r_1) {
					resolve((
						b ,
						c.$data.StringProp(r_1).StringProp()
					));
				}, reject)
			);
		}, reject);
	});
}(this, ko.contextFor(this)))";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_PromiseReturningTranslatedCall_NoReorder()
        {
            var result = CompileBinding("StringProp = _ext.Test(StringProp, StringProp = injectedService.Load(StringProp))", niceMode: true, new[] { typeof(TestViewModel) });

            var control = @"(function(a, b) {
	return new Promise(function(resolve, reject) {
		dotvvm.staticCommandPostback(a, ""WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiTG9hZCIsW10sIkFRQT0iXQ=="", [b.$data.StringProp()], options).then(function(r_0) {
			MethodExtensions.test(b.$data.StringProp, b.$data.StringProp(r_0).StringProp).then(function(r_1) {
				resolve(b.$data.StringProp(r_1).StringProp());
			}, reject);
		}, reject);
	});
}(this, ko.contextFor(this)))";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_PromiseReturningTranslatedCall_NeedsReorder()
        {
            var result = CompileBinding("StringProp = _ext.Test(StringProp, \"a\") + injectedService.Load(StringProp)", niceMode: true, new[] { typeof(TestViewModel) });

            var control = @"(function(a, c, b) {
	return new Promise(function(resolve, reject) {
		(
			b = dotvvm.staticCommandPostback(a, ""WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiTG9hZCIsW10sIkFRQT0iXQ=="", [c.$data.StringProp()], options) ,
			MethodExtensions.test(c.$data.StringProp, ""a"").then(function(r_0) {
				b.then(function(r_1) {
					resolve(c.$data.StringProp(r_0 + r_1).StringProp());
				}, reject);
			}, reject)
		);
	});
}(this, ko.contextFor(this)))";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_MarkupControlCommandPropertyUsed_SimpleCall_CorrectCommandExecturionOrder()
        {
            TestMarkupControl.CreateInitialized();

            var result = CompileBinding("_control.Save()", niceMode: true, new[] { typeof(object) }, typeof(Command), typeof(TestMarkupControl));

            var expectedReslt = @"
(function(a) {
	return new Promise(function(resolve, reject) {
		Promise.resolve(a.$control.Save()).then(function(r_0) {
			resolve(r_0);
		}, reject);
	});
}(ko.contextFor(this)))
";

            AreEqual(expectedReslt, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_MarkupControlCommandPropertyUsed_AsArgument_CorrectCommandExecturionOrder()
        {
            TestMarkupControl.CreateInitialized();

            var result = CompileBinding("injectedService.Load(_control.Load())", niceMode: true, new[] { typeof(object) }, typeof(Command), typeof(TestMarkupControl));

            var expectedReslt = @"
(function(a, b) {
	return new Promise(function(resolve, reject) {
		Promise.resolve(a.$control.Load()).then(function(r_0) {
			dotvvm.staticCommandPostback(b, ""WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiTG9hZCIsW10sIkFRQT0iXQ=="", [r_0], options).then(function(r_1) {
				resolve(r_1);
			}, reject);
		}, reject);
	});
}(ko.contextFor(this), this))
";

            AreEqual(expectedReslt, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_MarkupControlCommandPropertyUsed_WithSamePropertyDependancy_CorrectCommandExecturionOrder()
        {
            TestMarkupControl.CreateInitialized();

            var result = CompileBinding("StringProp = _control.Chanege(StringProp) + injectedService.Load(StringProp)", niceMode: true, new[] { typeof(TestViewModel) }, typeof(Command), typeof(TestMarkupControl));

            var expectedReslt = @"
(function(a, c, b) {
	return new Promise(function(resolve, reject) {
		(
			b = dotvvm.staticCommandPostback(a, ""WARNING/NOT/ENCRYPTED+++WyJEb3RWVk0uRnJhbWV3b3JrLlRlc3RzLkJpbmRpbmcuVGVzdFNlcnZpY2UsIERvdFZWTS5GcmFtZXdvcmsuVGVzdHMuQ29tbW9uIiwiTG9hZCIsW10sIkFRQT0iXQ=="", [c.$data.StringProp()], options) ,
			Promise.resolve(c.$control.Chanege(c.$data.StringProp())).then(function(r_0) {
				b.then(function(r_1) {
					resolve(c.$data.StringProp(r_0 + r_1).StringProp());
				}, reject);
			}, reject)
		);
	});
}(this, ko.contextFor(this)))";

            AreEqual(expectedReslt, result);
        }


        public void AreEqual(string expected, string actual)
        => Assert.AreEqual(RemoveWhitespaces(expected), RemoveWhitespaces(actual));

        public string RemoveWhitespaces(string source) => string.Concat(source.Where(c => !char.IsWhiteSpace(c)));
    }

    public class TestMarkupControl : DotvvmMarkupControl
    {
        public Command Save
        {
            get => (Command)GetValue(SaveProperty);
            set => SetValue(SaveProperty, value);
        }
        public static readonly DotvvmProperty SaveProperty
            = DotvvmProperty.Register<Command, TestMarkupControl>(c => c.Save, null);

        public Func<string> Load
        {
            get { return (Func<string>)GetValue(LoadProperty); }
            set { SetValue(LoadProperty, value); }
        }
        public static readonly DotvvmProperty LoadProperty
            = DotvvmProperty.Register<Func<string>, TestMarkupControl>(c => c.Load, null);

        public Func<string, string> Chanege
        {
            get { return (Func<string, string>)GetValue(ChanegeProperty); }
            set { SetValue(ChanegeProperty, value); }
        }
        public static readonly DotvvmProperty ChanegeProperty
            = DotvvmProperty.Register<Func<string, string>, TestMarkupControl>(c => c.Chanege, null);


        public static TestMarkupControl CreateInitialized()
        {
            var control = new TestMarkupControl();
            control.SetBinding(SaveProperty, new FakeCommandBinding(new ParametrizedCode("test"), null));
            control.SetBinding(LoadProperty, new FakeCommandBinding(new ParametrizedCode("test2"), null));
            control.SetBinding(ChanegeProperty, new FakeCommandBinding(new ParametrizedCode("test3"), null));
            return control;
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
        [AllowStaticCommand]
        public string Load(int integer) => null;
        [AllowStaticCommand]
        public string Load() => null;
        [AllowStaticCommand]
        public void Save(int integer) { }
        [AllowStaticCommand]
        public Task SaveAsync(int integer) => Task.CompletedTask;
        [AllowStaticCommand]
        public Task<string> LoadAsync() => Task.FromResult("");
    }

    public static class JsMethodsExtensions
    {
        public static string Test(string param, string param2)
        {
            throw new NotImplementedException();
        }
    }

}
