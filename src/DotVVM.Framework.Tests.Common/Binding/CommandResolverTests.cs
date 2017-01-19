using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Commands;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class CommandResolverTests
    {
        private DotvvmConfiguration configuration;
        private BindingCompilationService bindingService;

        [TestInitialize]
        public void INIT()
        {
            this.configuration = DotvvmConfiguration.CreateDefault();
            this.bindingService = configuration.ServiceLocator.GetService<BindingCompilationService>();
        }

        [TestMethod]
        public void CommandResolver_Valid_SimpleTest()
        {
            var path = new[] { ValueBindingExpression.CreateBinding<object>(bindingService, vm => ((dynamic)vm[0]).A[0], new ParametrizedCode("A()[0]")) };
            var commandId = "someCommand";
            var command = new CommandBindingExpression(bindingService, vm => ((TestA)vm[0]).Test(((TestA)vm[0]).StringToPass, ((dynamic)vm[1]).NumberToPass), commandId);

            var testObject = new {
                A = new[]
                {
                    new TestA() { StringToPass = "test" }
                },
                NumberToPass = 16
            };
            var viewRoot = new DotvvmView() { DataContext = testObject };
            viewRoot.SetBinding(Validation.TargetProperty, ValueBindingExpression.CreateBinding(bindingService, vm => vm.Last(), new ParametrizedCode("$root")));

            var placeholder = new HtmlGenericControl("div");
            placeholder.SetBinding(DotvvmBindableObject.DataContextProperty, path[0]);
            viewRoot.Children.Add(placeholder);

            var button = new Button();
            button.SetBinding(ButtonBase.ClickProperty, command);
            placeholder.Children.Add(button);

            var resolver = new CommandResolver();
            var context = new DotvvmRequestContext() { ViewModel = testObject };
            context.ModelState.ValidationTargetPath = KnockoutHelper.GetValidationTargetExpression(button);

            resolver.GetFunction(viewRoot, context, path.Select(v => v.GetKnockoutBindingExpression()).ToArray(), commandId, new object[0]).Action();

            Assert.AreEqual(testObject.NumberToPass, testObject.A[0].ResultInt);
            Assert.AreEqual(testObject.A[0].ResultString, testObject.A[0].ResultString);
        }

        public class TestA
        {
            public string StringToPass { get; set; }

            public string ResultString { get; set; }

            public int ResultInt { get; set; }

            public void Test(string s, int i)
            {
                ResultString = s;
                ResultInt = i;
            }
        }
    }
}
