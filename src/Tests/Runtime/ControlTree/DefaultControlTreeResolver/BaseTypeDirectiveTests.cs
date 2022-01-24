using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using System.Threading.Tasks;

namespace DotVVM.Framework.Tests.Runtime.ControlTree
{
    [TestClass]
    public class BaseTypeDirectiveTests : DefaultControlTreeResolverTestsBase
    {
        [TestMethod]
        public void ResolvedTree_BaseTypeDirective_CorrectBindings()
        {
            var root = ParseSource(@$"
@viewModel object
@baseType DotVVM.Framework.Tests.Runtime.ControlTree.{nameof(TestControl)}

<dot:TextBox Text={{value: _control.MyProperty }} />
<dot:Button Click={{staticCommand: _control.MyCommand()}} Text=""Test"" />
");
            CheckServiceAndBinding(root);
        }

        [TestMethod]
        public void ResolvedTree_BaseTypeDirective_CorrectBindings_UsingImportedNamespace()
        {
            var root = ParseSource(@$"
@viewModel object
@import DotVVM.Framework.Tests.Runtime.ControlTree
@baseType {nameof(TestControl)}

<dot:TextBox Text={{value: _control.MyProperty }} />
<dot:Button Click={{staticCommand: _control.MyCommand()}} Text=""Test"" />
");
            CheckServiceAndBinding(root);
        }

        [TestMethod]
        public void ResolvedTree_BaseTypeDirective_CorrectBindings_UsingAliasedType()
        {
            var root = ParseSource(@$"
@viewModel object
@import controlAlias = DotVVM.Framework.Tests.Runtime.ControlTree.{nameof(TestControl)}
@baseType controlAlias

<dot:TextBox Text={{value: _control.MyProperty }} />
<dot:Button Click={{staticCommand: _control.MyCommand()}} Text=""Test"" />
");
            CheckServiceAndBinding(root);
        }

        private static void CheckServiceAndBinding(ResolvedTreeRoot root)
        {
            var baseTypeDirerctive = EnsureSingleResolvedBaseTypeDirective(root);
            var textBinding = EnsureTextResolvedBinding(root);
            var clickBinding = EnsureClickResolvedBinding(root);

            Assert.IsFalse(baseTypeDirerctive.DothtmlNode.HasNodeErrors);
            Assert.IsNotNull(baseTypeDirerctive.ResolvedType);
            Assert.AreEqual(baseTypeDirerctive.ResolvedType.FullName, typeof(TestControl).FullName);

            Assert.AreEqual(typeof(string).FullName, textBinding.ResultType.FullName);
            Assert.IsFalse(textBinding.Errors.HasErrors);

            Assert.AreEqual(typeof(Task).FullName, clickBinding.ResultType.FullName);
            Assert.IsFalse(clickBinding.Errors.HasErrors);
        }

        private static ResolvedBaseTypeDirective EnsureSingleResolvedBaseTypeDirective(ResolvedTreeRoot root) =>
                root.Directives["baseType"]
                    .Single()
                    .CastTo<ResolvedBaseTypeDirective>();

        private static ResolvedBinding EnsureTextResolvedBinding(ResolvedTreeRoot root)
              => EnsureTextResolvedBinding(root, 2);

        private static ResolvedBinding EnsureClickResolvedBinding(ResolvedTreeRoot root)
              => EnsureTextResolvedBinding(root, 4);

        private static ResolvedBinding EnsureTextResolvedBinding(ResolvedTreeRoot root, int controlIndex) =>
              root.Content[0].CastTo<ResolvedControl>()
                  .Content[controlIndex].CastTo<ResolvedControl>()
                  .Properties.First().Value.CastTo<ResolvedPropertyBinding>()
                  .Binding; 
    }

    public class TestControl : DotvvmMarkupControl
    {
        public string MyProperty
        {
            get { return (string)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }
        public static readonly DotvvmProperty MyPropertyProperty
            = DotvvmProperty.Register<string, TestControl>(c => c.MyProperty, null);

        public Command MyCommand
        {
            get { return (Command)GetValue(MyCommandProperty); }
            set { SetValue(MyCommandProperty, value); }
        }
        public static readonly DotvvmProperty MyCommandProperty
            = DotvvmProperty.Register<Command, TestControl>(c => c.MyCommand, null);
    }
}
