using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Tests.Parser.Dothtml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.Runtime
{
    [TestClass]
    public class DefaultClientModuleCompilerTests
    {

        [TestMethod]
        public void DefaultClientModuleCompiler_EmptyBlock()
        {
            var code = @"";
            CompileModule(code, typeof(object));
        }

        [TestMethod]
        public void DefaultClientModuleCompiler_SingleMethod()
        {
            var code = @"public bool True() { return true; }";
            var module = CompileModule(code, typeof(object));
            Assert.IsTrue(module.True());
        }

        [TestMethod]
        public void DefaultClientModuleCompiler_MultipleMethods()
        {
            var code = @"public bool True() { return true; } public int Increment(int a) { return a + 1; }";
            var module = CompileModule(code, typeof(object));
            Assert.IsTrue(module.True());
            Assert.AreEqual(5, module.Increment(4));
        }

        [TestMethod]
        public void DefaultClientModuleCompiler_SimpleMethodTranslation()
        {
            var code = @"public void Increment(ClientModuleTestViewModel viewModel) { viewModel.IntProp = viewModel.IntProp + 1; }";
            var js = CompileAndTranslateModule(code, typeof(ClientModuleTestViewModel), new[] { "System" });
            Assert.AreEqual(js, @"dotvvm.clientModules = dotvvm.clientModules || {};
dotvvm.clientModules['_root'] = {
 'Increment': (function(viewModel){ko.unwrap(viewModel).IntProp(ko.unwrap(viewModel).IntProp()+1);return null;}),
}
");
        }

        [TestMethod]
        public void DefaultClientModuleCompiler_TwoMethods()
        {
            var code = @"public void Increment(ClientModuleTestViewModel viewModel) { viewModel.IntProp = viewModel.IntProp + 1; }
public void SetText(ClientModuleTestViewModel viewModel) { viewModel.StrProp = ""aaa""; }";
            var js = CompileAndTranslateModule(code, typeof(ClientModuleTestViewModel), new[] { "System" });
            Assert.AreEqual(js, @"dotvvm.clientModules = dotvvm.clientModules || {};
dotvvm.clientModules['_root'] = {
 'Increment': (function(viewModel){ko.unwrap(viewModel).IntProp(ko.unwrap(viewModel).IntProp()+1);return null;}),
 'SetText': (function(viewModel){ko.unwrap(viewModel).StrProp(""aaa"");return null;}),
}
");
        }



        private dynamic CompileModule(string code, Type viewModelType, string[] usings = null)
        {
            var module = DotvvmTestHelper.CompileClientModule(code, viewModelType, usings);
            return Activator.CreateInstance(module.Type);
        }

        private string CompileAndTranslateModule(string code, Type viewModelType, string[] usings = null)
        {
            var module = DotvvmTestHelper.CompileClientModule(code, viewModelType, usings);
            return DotvvmTestHelper.TranslateClientModule(module, viewModelType);
        }

    }

    public class ClientModuleTestViewModel
    {

        public int IntProp { get; set; }

        public string StrProp { get; set; }

    }
}
