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
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class StaticCommandValidationPathTests
    {

        static readonly BindingTestHelper bindingHelper = new BindingTestHelper(defaultExtensionParameters: new BindingExtensionParameter[] {
            new CurrentCollectionIndexExtensionParameter(),
            new BindingPageInfoExtensionParameter(),
            new InjectedServiceExtensionParameter("s", new ResolvedTypeDescriptor(typeof(ValidatedService)))
        });
        public string[] GetValidationPaths(string expression, params Type[] contexts) => GetValidationPaths(expression, contexts, expectedType: typeof(Command));
        public string[] GetValidationPaths(string expression, Type[] contexts, Type expectedType, Type currentMarkupControl = null)
        {
            var context = bindingHelper.CreateDataContext(contexts, markupControl: currentMarkupControl);
            var staticCommand = bindingHelper.StaticCommand(expression, context, expectedType);
            
            var jsExpression = staticCommand.GetProperty<StaticCommandJsAstProperty>().Expression;
            var validationPaths = jsExpression.DescendantNodesAndSelf().OfType<JsInvocationExpression>()
                .Where(i => i.Arguments.Count >= 4 && i.Target.ToString() == "dotvvm.staticCommandPostback")
                .Select(i => i.Arguments.ElementAt(3) as JsArrayExpression)
                .Where(i => i != null)
                .SelectMany(i => i.Children)
                .Select(t => t.FormatParametrizedScript(niceMode: true).ToString(p => p.HasDefault ? p.DefaultAssignment : new CodeParameterAssignment("$par", OperatorPrecedence.Max)))
                .ToArray();

            return validationPaths;
        }

        [TestMethod]
        public void BasicProperties()
        {
            var p = GetValidationPaths("s.Method(_this.TestViewModel2.MyProperty)", typeof(TestViewModel));
            Assert.AreEqual("\"TestViewModel2/MyProperty\"", p.Single());
        }

        [TestMethod]
        public void RenamedProperty()
        {
            // the paths are evaluated client-side, so we must use the client-side name
            var p = GetValidationPaths("s.Method(_this.MyProperty)", typeof(ViewModelWithRenamedProperty));
            Assert.AreEqual("\"x\"", p.Single());
        }

        [TestMethod]
        public void ArrayIndex()
        {
            var p = GetValidationPaths("s.Method(_this.VmArray[_this.IntProp].MyProperty)", typeof(TestViewModel));
            Assert.AreEqual("\"VmArray/\" + $par.IntProp.state + \"/MyProperty\"", p.Single());
        }

        [TestMethod]
        public void ListIndex()
        {
            var p = GetValidationPaths("s.Method(_this.StringList[_this.IntProp])", typeof(TestViewModel));
            Assert.AreEqual("\"StringList/\" + $par.IntProp.state", p.Single());
        }

        [DataTestMethod]
        [DataRow("_this", "\".\"")]
        [DataRow("_parent", "\"..\"")]
        [DataRow("_root", "\"../..\"")]
        [DataRow("_root.DateTime", "\"../../DateTime\"")]
        public void ParentReference(string expr, string expected)
        {
            var p = GetValidationPaths($"s.Method({expr})", new [] { typeof(TestViewModel), typeof(string), typeof(string) });
            Assert.AreEqual(expected, p.Single());
        }

        [DataTestMethod]
        [DataRow("!BoolProp", "\"BoolProp\"")]
        [DataRow("_this.DateTime.ToBrowserLocalTime()", "\"DateTime\"")]
        [DataRow("_this.DateTime.ToString('blabla')", "\"DateTime\"")]
        [DataRow("Enums.ToEnumString(_this.EnumProperty)", "\"EnumProperty\"")]
        [DataRow("_this.IntProp", "\"IntProp\"")] // this will use BoxingUtils.Box
        [DataRow("_this.IntProp + 1", "\"IntProp\"")]
        [DataRow("'padding: ' + _this.StringProp", "\"StringProp\"")]
        public void UnwrapsPointlessExpressions(string expr, string expected)
        {
            var p = GetValidationPaths($"s.Method({expr})", typeof(TestViewModel));
            Assert.AreEqual(expected, p.Single());
        }

        [TestMethod]
        public void PrintsUnsupportedReason()
        {
            var p = GetValidationPaths("s.Method(s.OtherMethod())", typeof(TestViewModel));
            Assert.AreEqual("/* Expression Call (s.OtherMethod()) isn't supported */ null", p.Single());
        }

        [TestMethod]
        public void UnsupportedDictionary()
        {
            var p = GetValidationPaths("s.Method(StringVmDictionary['aa'])", typeof(TestViewModel));
            Assert.AreEqual("/* Unsupported index */ null", p.Single());
        }


        [TestMethod]
        public void ConditionalProperty()
        {
            var p = GetValidationPaths("s.Method(StringProp != 'aaa' ? StringProp : TestViewModel2.SomeString)", typeof(TestViewModel));
            Assert.AreEqual("$par.StringProp.state != \"aaa\" ? \"StringProp\" : \"TestViewModel2/SomeString\"", p.Single());
        }

        [TestMethod]
        public void ConditionalPropertyIgnoresEdgecase()
        {
            var p = GetValidationPaths("s.Method(StringProp != 'aaa' ? StringProp : 'bb')", typeof(TestViewModel));
            Assert.AreEqual("\"StringProp\"", p.Single());
        }

        [TestMethod]
        public void ConditionalPropertyDoesntExecuteCommandTwice()
        {
            // BoolMethod is a static command, so we really don't want to execute it twice just to evaluate the path
            var p = GetValidationPaths("s.Method(s.BoolMethod() ? StringProp : TestViewModel2.SomeString)", typeof(TestViewModel));
            Assert.AreEqual("/* Unsupported condition */ null", p.Single());
        }


        [TestMethod]
        public async Task Validation_CompiledBinding_ConstantArgument()
        {
            var exception = await Assert.ThrowsExceptionAsync<DotvvmInvalidStaticCommandModelStateException>(() =>
                InvokeCompiledBinding("s.Save('fixed', Text)", ""));

            Assert.AreEqual("/Text", XAssert.Single(exception.StaticCommandModelState.Errors).PropertyPath);
        }

        [TestMethod]
        public async Task Validation_CompiledBinding_InstanceReceiver()
        {
            var vm = new ValidationViewModel { Text = "valid" };
            var result = await InvokeCompiledBinding("_this.Save(Text)", vm, vm.Text);

            Assert.AreEqual("valid", result);
        }

        async Task<object> InvokeCompiledBinding(string expression, params object[] clientArguments)
        {
            var config = DotvvmTestHelper.CreateConfiguration(s => s.AddSingleton<ValidatedService>());
            var binding = bindingHelper.StaticCommand(expression, new[] { typeof(ValidationViewModel) });
            var call = binding.GetProperty<StaticCommandJsAstProperty>().Expression.DescendantNodesAndSelf()
                .OfType<JsInvocationExpression>().Single(i => i.Target.ToString() == "dotvvm.staticCommandPostback");
            var plan = call.Annotation<StaticCommandMethodTranslator.StaticCommandInvocationJsAnnotation>().Plan;
            var compiledArguments = (JsArrayExpression)call.Arguments.ElementAt(1);
            var relativePaths = GetValidationPaths(expression, typeof(ValidationViewModel))
                .Select(p => JsonSerializer.Deserialize<string>(p)).ToArray();

            Assert.AreEqual(compiledArguments.Children.Count(), clientArguments.Length);
            var arguments = JsonSerializer.SerializeToElement(clientArguments);
            Console.WriteLine("Argument plan: " + string.Join(", ", plan.Arguments.Select(a => a.Type)));
            Console.WriteLine("Client arguments: " + arguments.GetRawText());
            Console.WriteLine("Compiled validation paths: " + JsonSerializer.Serialize(relativePaths));

            // These bindings use literal paths in the root context. Resolve them as the client does:
            // null stays null, '.' becomes '/', and 'Text' becomes '/Text'.
            var absolutePaths = relativePaths.Select(p => p == null ? null : p == "." ? "/" : "/" + p);
            var context = DotvvmTestHelper.CreateContext(config, requestType: DotvvmRequestType.StaticCommand);
            return await config.ServiceProvider.GetRequiredService<StaticCommandExecutor>()
                .Execute(plan, arguments, absolutePaths, context);
        }

        public class ValidationViewModel
        {
            [Required]
            public string Text { get; set; }

            [AllowStaticCommand(StaticCommandValidation.Automatic)]
            public string Save([Required] string text) => text;
        }

        public class ValidatedService
        {
            [AllowStaticCommand(StaticCommandValidation.Automatic)]
            public string Save(string prefix, [Required] string text) => prefix + text;

            [AllowStaticCommand(StaticCommandValidation.Manual)]
            public void Method(object a) { }

            [AllowStaticCommand]
            public string OtherMethod() { return "aaa"; }

            [AllowStaticCommand]
            public bool BoolMethod() { return false; }
        }

        public class ViewModelWithRenamedProperty
        {
            [Bind(Name = "x")]
            public string MyProperty { get; set; }
        }
    }
}
