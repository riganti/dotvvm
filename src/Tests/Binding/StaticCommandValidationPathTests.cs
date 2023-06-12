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
    public class StaticCommandValidationPathTests
    {

        static readonly BindingTestHelper bindingHelper = new BindingTestHelper(defaultExtensionParameters: new BindingExtensionParameter[] {
            new CurrentCollectionIndexExtensionParameter(),
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

        [TestMethod]
        public void ToBrowserLocalTime()
        {
            var p = GetValidationPaths("s.Method(_this.DateTime.ToBrowserLocalTime())", typeof(TestViewModel));
            Assert.AreEqual("\"DateTime\"", p.Single());
        }

        [TestMethod]
        public void ParentReference()
        {
            var p = GetValidationPaths("s.Method(_root.DateTime.ToString())", new [] { typeof(TestViewModel), typeof(string), typeof(string) });
            Assert.AreEqual("\"../../DateTime\"", p.Single());
        }

        [TestMethod]
        public void UnwrapsNegation()
        {
            var p = GetValidationPaths("s.Method(!BoolProp)", typeof(TestViewModel));
            Assert.AreEqual("\"BoolProp\"", p.Single());
        }

        [TestMethod]
        public void PrintsUnsupportedReason()
        {
            var p = GetValidationPaths("s.Method(s.OtherMethod())", typeof(TestViewModel));
            Assert.AreEqual("/* Expression Call (s.OtherMethod()) isn't supported */ null", p.Single());
        }

        public class ValidatedService
        {
            [AllowStaticCommand(StaticCommandValidation.Manual)]
            public void Method(object a) { }

            [AllowStaticCommand]
            public string OtherMethod() { return "aaa"; }
        }
    }
}
