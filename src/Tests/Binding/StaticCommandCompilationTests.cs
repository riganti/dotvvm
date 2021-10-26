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
using DotVVM.Framework.Binding.Properties;

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
                s.Configure<BindingCompilationOptions>(o => {
                    o.TransformerClasses.OfType<BindingPropertyResolvers>().First().AddNullChecks = false;
                });
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
            configuration.Debug = niceMode;
            var bindingService = configuration.ServiceProvider.GetRequiredService<BindingCompilationService>();

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

            var staticCommand = new StaticCommandBindingExpression(bindingService, new object[] {
                new OriginalStringBindingProperty(expression),
                context,
                options,
                new ExpectedTypeBindingProperty(expectedType)
            });
            var expr = KnockoutHelper.GenerateClientPostBackExpression(
                "",
                staticCommand,
                new Literal(),
                new PostbackScriptOptions(
                    allowPostbackHandlers: false,
                    returnValue: null,
                    commandArgs: CodeParameterAssignment.FromIdentifier("commandArguments")
                ));
            if (expr.StartsWith("dotvvm.applyPostbackHandlers(") && expr.EndsWith(",this,[],commandArguments)"))
                expr = expr.Substring(29, expr.Length - 29 - 26);
            if (expr.StartsWith("async"))
                expr = expr.Substring("async".Length).TrimStart();
            if (expr.StartsWith("(options)"))
                expr = expr.Substring("(options)".Length).TrimStart();
            if (expr.StartsWith("=>"))
                expr = expr.Substring("=>".Length).TrimStart();
            return expr;
        }

        [TestMethod]
        public void StaticCommandCompilation_SimpleCommand()
        {
            var result = CompileBinding("StaticCommands.GetLength(StringProp)", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("{await dotvvm.staticCommandPostback(\"XXXX\",[options.viewModel.StringProp.state],options);}", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_AssignedCommand()
        {
            var result = CompileBinding("StringProp = StaticCommands.GetLength(StringProp).ToString()", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("{let vm=options.viewModel;vm.StringProp(dotvvm.globalize.bindingNumberToString(await dotvvm.staticCommandPostback(\"XXXX\",[vm.StringProp()],options))());}", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_JsOnlyCommand()
        {
            var result = CompileBinding("StringProp = StringProp.Length.ToString()", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("{let vm=options.viewModel;vm.StringProp(dotvvm.globalize.bindingNumberToString(vm.StringProp().length)());}", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ChainedCommands()
        {
            var result = CompileBinding("StringProp = StaticCommands.GetLength(StaticCommands.GetLength(StringProp).ToString()).ToString()", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("{let vm=options.viewModel;vm.StringProp(dotvvm.globalize.bindingNumberToString(await dotvvm.staticCommandPostback(\"XXXX\",[dotvvm.globalize.bindingNumberToString(await dotvvm.staticCommandPostback(\"XXXX\",[vm.StringProp()],options))()],options))());}", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_MultipleCommandsWithVariable()
        {
            var result = CompileBinding("var lenVar = StaticCommands.GetLength(StringProp).ToString(); StringProp = StaticCommands.GetLength(lenVar).ToString();", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("{let vm=options.viewModel;let lenVar=dotvvm.globalize.bindingNumberToString(await dotvvm.staticCommandPostback(\"XXXX\",[vm.StringProp()],options))();vm.StringProp(dotvvm.globalize.bindingNumberToString(await dotvvm.staticCommandPostback(\"XXXX\",[lenVar],options))());}", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ChainedCommandsWithSemicolon()
        {
            var result = CompileBinding("StringProp = StaticCommands.GetLength(StringProp).ToString(); StringProp = StaticCommands.GetLength(StringProp).ToString()", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("{let vm=options.viewModel;vm.StringProp(dotvvm.globalize.bindingNumberToString(await dotvvm.staticCommandPostback(\"XXXX\",[vm.StringProp()],options))());vm.StringProp(dotvvm.globalize.bindingNumberToString(await dotvvm.staticCommandPostback(\"XXXX\",[vm.StringProp()],options))());}", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_DateTimeResultAssignment()
        {
            var result = CompileBinding("DateFrom = StaticCommands.GetDate()", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("{options.viewModel.DateFrom(dotvvm.serialization.serializeDate(await dotvvm.staticCommandPostback(\"XXXX\",[],options),false));}", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_DateTimeAssignment()
        {
            var result = CompileBinding("DateFrom = DateTo", niceMode: false, typeof(TestViewModel));
            Assert.AreEqual("{let vm=options.viewModel;vm.DateFrom(dotvvm.serialization.serializeDate(vm.DateTo.state,false));}", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_CommandArgumentUsage()
        {
            var result = CompileBinding("StringProp = arg.ToString()", niceMode: false, new[] { typeof(TestViewModel) }, typeof(Func<int, Task>));
            Assert.AreEqual("{options.viewModel.StringProp(dotvvm.globalize.bindingNumberToString(commandArguments[0])());}", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_PossibleAmniguousMatch()
        {
            var result = CompileBinding("SomeString = injectedService.Load(SomeString)", niceMode: false, new[] { typeof(TestViewModel3) }, typeof(Func<string, string>));

            Assert.AreEqual("{let vm=options.viewModel;return vm.SomeString(await dotvvm.staticCommandPostback(\"XXXX\",[vm.SomeString.state],options)).SomeString();}", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_IndexParameter()
        {
            var result = CompileBinding("IntProp = _index", niceMode: false, new[] { typeof(TestViewModel) });
            Assert.AreEqual("{options.viewModel.IntProp(options.knockoutContext.$index());}", result);
        }


        [TestMethod]
        public void StaticCommandCompilation_ListIndexer()
        {
            var result = CompileBinding("LongList[1] = LongList[0] + LongArray[0]", niceMode: false, new[] { typeof(TestViewModel) });
            Assert.AreEqual("{let vm=options.viewModel;dotvvm.translations.array.setItem(vm.LongList,1,vm.LongList.state[0]+vm.LongArray.state[0]);}", result);
        }
        [TestMethod]
        public void StaticCommandCompilation_ArrayIndexer()
        {
            var result = CompileBinding("LongArray[1] = LongList[0] + LongArray[0]", niceMode: false, new[] { typeof(TestViewModel) });
            Assert.AreEqual("{let vm=options.viewModel;dotvvm.translations.array.setItem(vm.LongArray,1,vm.LongList.state[0]+vm.LongArray.state[0]);}", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_IndexParameterInParent()
        {
            var result = CompileBinding("_parent2.IntProp = _index", niceMode: false, new[] { typeof(TestViewModel), typeof(object), typeof(string) });
            Assert.AreEqual("{let cx=options.knockoutContext;cx.$parents[1].IntProp(cx.$parentContext.$parentContext.$index());}", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ExpressionBetweenPostbacks_WithParameters()
        {
            var result = CompileBinding("StringProp = injectedService.Load(StringProp, StringProp); \"Test\"; StringProp = injectedService.Load(StringProp)", niceMode: false, new[] { typeof(TestViewModel) });
            Assert.AreEqual("{let vm=options.viewModel;vm.StringProp(await dotvvm.staticCommandPostback(\"XXXX\",[vm.StringProp.state,vm.StringProp.state],options));\"Test\";vm.StringProp(await dotvvm.staticCommandPostback(\"XXXX\",[vm.StringProp.state],options));}", result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ExpressionBetweenPostbacks_NoParametersLast()
        {
            var result = CompileBinding("StringProp = injectedService.Load(IntProp); \"Test\"; StringProp2 = injectedService.Load()", niceMode: true, new[] { typeof(TestViewModel) });
            Console.WriteLine(result);
            var control = @"{
	let vm = options.viewModel;
	vm.StringProp(await dotvvm.staticCommandPostback(""XXXX"", [vm.IntProp.state], options));
	""Test"";
	vm.StringProp2(await dotvvm.staticCommandPostback(""XXXX"", [], options));
}";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ExpressionBetweenPostbacks_VoidTypeFirst()
        {
            var result = CompileBinding("injectedService.Save(IntProp); \"Test\"; StringProp = injectedService.Load()", niceMode: true, new[] { typeof(TestViewModel) });

            Console.WriteLine(result);
            var control = @"{
	let vm = options.viewModel;
	await dotvvm.staticCommandPostback(""XXXX"", [vm.IntProp.state], options);
	""Test"";
	vm.StringProp(await dotvvm.staticCommandPostback(""XXXX"", [], options));
}";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_ExpressionBetweenAsyncPostbacks_TaskTypeFirst()
        {
            var result = CompileBinding("injectedService.SaveAsync(IntProp); \"Test\"; StringProp = injectedService.LoadAsync().Result", niceMode: true, new[] { typeof(TestViewModel) });

            Console.WriteLine(result);
            var control = @"{
 	let vm = options.viewModel;
 	await dotvvm.staticCommandPostback(""XXXX"", [vm.IntProp.state], options);
 	""Test"";
 	return vm.StringProp(await dotvvm.staticCommandPostback(""XXXX"", [], options)).StringProp();
 }";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_DependentPostbacks_TaskTypeFirst()
        {
            var result = CompileBinding("StringProp = injectedService.Load(IntProp); StringProp = injectedService.Load(StringProp)", niceMode: true, new[] { typeof(TestViewModel) });

            Console.WriteLine(result);
            var control = @"{
 	let vm = options.viewModel;
    vm.StringProp(await dotvvm.staticCommandPostback(""XXXX"", [vm.IntProp.state], options));
 	vm.StringProp(await dotvvm.staticCommandPostback(""XXXX"", [vm.StringProp.state], options));
 }";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_PromiseReturningTranslatedCall_NoReorder()
        {
            var result = CompileBinding("StringProp = _ext.Test(StringProp, StringProp = injectedService.Load(StringProp))", niceMode: true, new[] { typeof(TestViewModel) });

            Console.WriteLine(result);
            var control = @"{
	let vm = options.viewModel;
	vm.StringProp(await MethodExtensions.test(vm.StringProp, vm.StringProp(await dotvvm.staticCommandPostback(""XXXX"", [vm.StringProp()], options)).StringProp));
}";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_PromiseReturningTranslatedCall_NeedsReorder()
        {
            var result = CompileBinding("StringProp = _ext.Test(StringProp, \"a\") + injectedService.Load(StringProp)", niceMode: true, new[] { typeof(TestViewModel) });

            Console.WriteLine(result);
            var control = @"{
	let vm = options.viewModel;
	vm.StringProp(await MethodExtensions.test(vm.StringProp, ""a"") + await dotvvm.staticCommandPostback(""XXXX"", [vm.StringProp.state], options));
}";

            AreEqual(control, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_MarkupControlCommandPropertyUsed_SimpleCall_CorrectCommandExecturionOrder()
        {
            TestMarkupControl.CreateInitialized();

            var result = CompileBinding("_control.Save()", niceMode: true, new[] { typeof(object) }, typeof(Command), typeof(TestMarkupControl));

            Console.WriteLine(result);
            var expectedResult = @"await options.knockoutContext.$control.Save.state()";

            AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_MarkupControlCommandPropertyUsed_AsArgument_CorrectCommandExecturionOrder()
        {
            TestMarkupControl.CreateInitialized();

            var result = CompileBinding("injectedService.Load(_control.Load())", niceMode: true, new[] { typeof(object) }, typeof(Command), typeof(TestMarkupControl));

            Console.WriteLine(result);
            var expectedResult = @"{
    await dotvvm.staticCommandPostback(""XXXX"", [await options.knockoutContext.$control.Load.state()], options);
}";

            AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_MarkupControlCommandPropertyUsed_WithSamePropertyDependency_CorrectCommandExecturionOrder()
        {
            TestMarkupControl.CreateInitialized();

            var result = CompileBinding("StringProp = _control.Change(StringProp) + injectedService.Load(StringProp)", niceMode: true, new[] { typeof(TestViewModel) }, typeof(Command), typeof(TestMarkupControl));

            Console.WriteLine(result);
            var expectedResult = @"{
	let vm = options.viewModel;
	vm.StringProp(await options.knockoutContext.$control.Change.state(vm.StringProp.state) + await dotvvm.staticCommandPostback(""XXXX"", [vm.StringProp.state], options));
}";

            AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void StaticCommandCompilation_LinqTranslations()
        {
            TestMarkupControl.CreateInitialized();

            var result = CompileBinding("StringProp = VmArray.Where(x => x.ChildObject.SomeString == 'x').FirstOrDefault().SomeString", niceMode: true, new[] { typeof(TestViewModel) }, typeof(Command));

            Console.WriteLine(result);
            var expectedResult = @"{
 	let vm = options.viewModel;
 	vm.StringProp(vm.VmArray.state.filter((x) => ko.unwrap(x).ChildObject.SomeString == ""x"")[0].SomeString);
 }";

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
