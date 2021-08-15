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
using System.Collections.Immutable;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Security;

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
            var configuration = DotvvmTestHelper.CreateConfiguration(s => {
                s.AddSingleton<IViewModelProtector, DotvvmTestHelper.NopProtector>();
            });

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

            var options = BindingParserOptions.StaticCommand
                .AddImports(configuration.Markup.ImportedNamespaces);

            var parser = new BindingExpressionBuilder(configuration.ServiceProvider.GetRequiredService<CompiledAssemblyCache>(), configuration.ServiceProvider.GetRequiredService<ExtensionMethodsCache>());
            var expressionTree = parser.ParseWithLambdaConversion(expression, context, options, expectedType);
            var jsExpression =
                configuration.ServiceProvider.GetRequiredService<StaticCommandBindingCompiler>().CompileToJavascript(context, expressionTree);
            return KnockoutHelper.GenerateClientPostBackExpression(
                "",
                new FakeCommandBinding(BindingPropertyResolvers.FormatJavascript(jsExpression, allowObservableResult: false, nullChecks: false, niceMode: niceMode), null),
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
            Assert.AreEqual("(async function(a){return await dotvvm.staticCommandPostback(\"XXXX\",[a.$data.StringProp.state],options);}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_AssignedCommand()
        {
            var result = CompileBinding("StringProp = StaticCommands.GetLength(StringProp).ToString()", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("(async function(a){return a.$data.StringProp(dotvvm.globalize.bindingNumberToString(await dotvvm.staticCommandPostback(\"XXXX\",[a.$data.StringProp()],options))()).StringProp();}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_JsOnlyCommand()
        {
            var result = CompileBinding("StringProp = StringProp.Length.ToString()", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("(function(a){return a.$data.StringProp(dotvvm.globalize.bindingNumberToString(a.$data.StringProp().length)()).StringProp();}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ChainedCommands()
        {
            var result = CompileBinding("StringProp = StaticCommands.GetLength(StaticCommands.GetLength(StringProp).ToString()).ToString()", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("(async function(a){return a.$data.StringProp(dotvvm.globalize.bindingNumberToString(await dotvvm.staticCommandPostback(\"XXXX\",[dotvvm.globalize.bindingNumberToString(await dotvvm.staticCommandPostback(\"XXXX\",[a.$data.StringProp()],options))()],options))()).StringProp();}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_MultipleCommandsWithVariable()
        {
            var result = CompileBinding("var lenVar = StaticCommands.GetLength(StringProp).ToString(); StringProp = StaticCommands.GetLength(lenVar).ToString();", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("(async function(a,b){return (b=dotvvm.globalize.bindingNumberToString(await dotvvm.staticCommandPostback(\"XXXX\",[a.$data.StringProp()],options))(),a.$data.StringProp(dotvvm.globalize.bindingNumberToString(await dotvvm.staticCommandPostback(\"XXXX\",[b],options))()).StringProp(),null);}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ChainedCommandsWithSemicolon()
        {
            var result = CompileBinding("StringProp = StaticCommands.GetLength(StringProp).ToString(); StringProp = StaticCommands.GetLength(StringProp).ToString()", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("(async function(a){return (a.$data.StringProp(dotvvm.globalize.bindingNumberToString(await dotvvm.staticCommandPostback(\"XXXX\",[a.$data.StringProp()],options))()).StringProp(),a.$data.StringProp(dotvvm.globalize.bindingNumberToString(await dotvvm.staticCommandPostback(\"XXXX\",[a.$data.StringProp()],options))()).StringProp());}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_DateTimeResultAssignment()
        {
            var result = CompileBinding("DateFrom = StaticCommands.GetDate()", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("(async function(a){return a.$data.DateFrom(dotvvm.serialization.serializeDate(await dotvvm.staticCommandPostback(\"XXXX\",[],options),false)).DateFrom();}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_DateTimeAssignment()
        {
            var result = CompileBinding("DateFrom = DateTo", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("(function(a){return a.$data.DateFrom(dotvvm.serialization.serializeDate(a.$data.DateTo.state,false)).DateFrom();}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_CommandArgumentUsage()
        {
            var result = CompileBinding("StringProp = arg.ToString()", niceMode: false, new[] { typeof(TestViewModel) }, typeof(Func<int, Task>));
            Assert.AreEqual("(function(a){return a.$data.StringProp(dotvvm.globalize.bindingNumberToString(commandArguments[0])()).StringProp();}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_PossibleAmniguousMatch()
        {
            var result = CompileBinding("SomeString = injectedService.Load(SomeString)", niceMode: false, new[] { typeof(TestViewModel3) }, typeof(Func<string, string>));

            Assert.AreEqual("(async function(a){return a.$data.SomeString(await dotvvm.staticCommandPostback(\"XXXX\",[a.$data.SomeString.state],options)).SomeString();}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_IndexParameter()
        {
            var result = CompileBinding("IntProp = _index", niceMode: false, new[] { typeof(TestViewModel) });
            Assert.AreEqual("(function(a){return a.$data.IntProp(a.$index()).IntProp();}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_IndexParameterInParent()
        {
            var result = CompileBinding("_parent2.IntProp = _index", niceMode: false, new[] { typeof(TestViewModel), typeof(object), typeof(string) });
            Assert.AreEqual("(function(a){return a.$parents[1].IntProp(a.$parentContext.$parentContext.$index()).IntProp();}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ExpressionBetweenPostbacks_WithParameters()
        {
            var result = CompileBinding("StringProp = injectedService.Load(StringProp, StringProp); \"Test\"; StringProp = injectedService.Load(StringProp)", niceMode: false, new[] { typeof(TestViewModel) });
            Assert.AreEqual("(async function(a){return (a.$data.StringProp(await dotvvm.staticCommandPostback(\"XXXX\",[a.$data.StringProp.state,a.$data.StringProp.state],options)).StringProp(),\"Test\",a.$data.StringProp(await dotvvm.staticCommandPostback(\"XXXX\",[a.$data.StringProp.state],options)).StringProp());}(ko.contextFor(this)))", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ExpressionBetweenPostbacks_NoParametersLast()
        {
            var result = CompileBinding("StringProp = injectedService.Load(IntProp); \"Test\"; StringProp2 = injectedService.Load()", niceMode: true, new[] { typeof(TestViewModel) });
            Console.WriteLine(result);
            var control = @"
(async function(a) {
	return (
		a.$data.StringProp(await dotvvm.staticCommandPostback(""XXXX"", [a.$data.IntProp.state], options)).StringProp() ,
		""Test"" ,
		a.$data.StringProp2(await dotvvm.staticCommandPostback(""XXXX"", [], options)).StringProp2()
	);
}(ko.contextFor(this)))";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ExpressionBetweenPostbacks_VoidTypeFirst()
        {
            var result = CompileBinding("injectedService.Save(IntProp); \"Test\"; StringProp = injectedService.Load()", niceMode: true, new[] { typeof(TestViewModel) });

            Console.WriteLine(result);
            var control = @"
(async function(a) {
	return (
		await dotvvm.staticCommandPostback(""XXXX"", [a.$data.IntProp.state], options) ,
		""Test"" ,
		a.$data.StringProp(await dotvvm.staticCommandPostback(""XXXX"", [], options)).StringProp()
	);
}(ko.contextFor(this)))";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ExpressionBetweenAsyncPostbacks_TaskTypeFirst()
        {
            var result = CompileBinding("injectedService.SaveAsync(IntProp); \"Test\"; StringProp = injectedService.LoadAsync().Result", niceMode: true, new[] { typeof(TestViewModel) });

            Console.WriteLine(result);
            var control = @"
(async function(a) {
	return (
		await dotvvm.staticCommandPostback(""XXXX"", [a.$data.IntProp.state], options) ,
		(
			""Test"" ,
			a.$data.StringProp(await dotvvm.staticCommandPostback(""XXXX"", [], options)).StringProp()
		)
	);
}(ko.contextFor(this)))";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_DependentPostbacks_TaskTypeFirst()
        {
            var result = CompileBinding("StringProp = injectedService.Load(IntProp); StringProp = injectedService.Load(StringProp)", niceMode: true, new[] { typeof(TestViewModel) });

            Console.WriteLine(result);
            var control = @"
(async function(a) {
	return (
		a.$data.StringProp(await dotvvm.staticCommandPostback(""XXXX"", [a.$data.IntProp.state], options)).StringProp() ,
		a.$data.StringProp(await dotvvm.staticCommandPostback(""XXXX"", [a.$data.StringProp.state], options)).StringProp()
	);
}(ko.contextFor(this)))";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_PromiseReturningTranslatedCall_NoReorder()
        {
            var result = CompileBinding("StringProp = _ext.Test(StringProp, StringProp = injectedService.Load(StringProp))", niceMode: true, new[] { typeof(TestViewModel) });

            Console.WriteLine(result);
            var control = @"
(async function(a) {
	return a.$data.StringProp(await MethodExtensions.test(a.$data.StringProp, a.$data.StringProp(await dotvvm.staticCommandPostback(""XXXX"", [a.$data.StringProp()], options)).StringProp)).StringProp();
}(ko.contextFor(this)))";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_PromiseReturningTranslatedCall_NeedsReorder()
        {
            var result = CompileBinding("StringProp = _ext.Test(StringProp, \"a\") + injectedService.Load(StringProp)", niceMode: true, new[] { typeof(TestViewModel) });

            Console.WriteLine(result);
            var control = @"
(async function(a) {
	return a.$data.StringProp(await MethodExtensions.test(a.$data.StringProp, ""a"") + await dotvvm.staticCommandPostback(""XXXX"", [a.$data.StringProp.state], options)).StringProp();
}(ko.contextFor(this)))";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_MarkupControlCommandPropertyUsed_SimpleCall_CorrectCommandExecturionOrder()
        {
            TestMarkupControl.CreateInitialized();

            var result = CompileBinding("_control.Save()", niceMode: true, new[] { typeof(object) }, typeof(Command), typeof(TestMarkupControl));

            Console.WriteLine(result);
            var expectedResult = @"
(async function(a) {
	return await Promise.resolve(a.$control.Save.state());
}(ko.contextFor(this)))";

            AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_MarkupControlCommandPropertyUsed_AsArgument_CorrectCommandExecturionOrder()
        {
            TestMarkupControl.CreateInitialized();

            var result = CompileBinding("injectedService.Load(_control.Load())", niceMode: true, new[] { typeof(object) }, typeof(Command), typeof(TestMarkupControl));

            Console.WriteLine(result);
            var expectedResult = @"
(async function(a) {
	return await dotvvm.staticCommandPostback(""XXXX"", [await Promise.resolve(a.$control.Load.state())], options);
}(ko.contextFor(this)))";

            AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_MarkupControlCommandPropertyUsed_WithSamePropertyDependency_CorrectCommandExecturionOrder()
        {
            TestMarkupControl.CreateInitialized();

            var result = CompileBinding("StringProp = _control.Change(StringProp) + injectedService.Load(StringProp)", niceMode: true, new[] { typeof(TestViewModel) }, typeof(Command), typeof(TestMarkupControl));

            Console.WriteLine(result);
            var expectedResult = @"
(async function(a) {
	return a.$data.StringProp(await Promise.resolve(a.$control.Change.state(a.$data.StringProp.state)) + await dotvvm.staticCommandPostback(""XXXX"", [a.$data.StringProp.state], options)).StringProp();
}(ko.contextFor(this)))";

            AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_LinqTranslations()
        {
            TestMarkupControl.CreateInitialized();

            var result = CompileBinding("StringProp = VmArray.Where(x => x.ChildObject.SomeString == 'x').FirstOrDefault().SomeString", niceMode: true, new[] { typeof(TestViewModel) }, typeof(Command));

            Console.WriteLine(result);
            var expectedResult = @"
 (function(a) {
 	return a.$data.StringProp(dotvvm.translations.array.firstOrDefault(a.$data.VmArray.state.filter(function(x) {
 		return ko.unwrap(x).ChildObject.SomeString == ""x"";
 	}), function(arg) {
 		return true;
 	}).SomeString).StringProp();
 }(ko.contextFor(this)))";

            AreEqual(expectedResult, result);
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

        public Func<string, string> Change
        {
            get { return (Func<string, string>)GetValue(ChangeProperty); }
            set { SetValue(ChangeProperty, value); }
        }
        public static readonly DotvvmProperty ChangeProperty
            = DotvvmProperty.Register<Func<string, string>, TestMarkupControl>(c => c.Change, null);


        public static TestMarkupControl CreateInitialized()
        {
            var control = new TestMarkupControl();
            control.SetBinding(SaveProperty, new FakeCommandBinding(new ParametrizedCode("test"), null));
            control.SetBinding(LoadProperty, new FakeCommandBinding(new ParametrizedCode("test2"), null));
            control.SetBinding(ChangeProperty, new FakeCommandBinding(new ParametrizedCode("test3"), null));
            return control;
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
