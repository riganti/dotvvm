using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class CommandResolverTests
    {

        [TestMethod]
        public void CommandResolver_Valid_SimpleTest()
        {
            var path = new[] { new ValueBindingExpression(vm => ((dynamic)vm[0]).A[0], "A()[0]") };
            var commandId = "someCommand";
            var command = new CommandBindingExpression(vm => ((TestA)vm[0]).Test(((TestA)vm[0]).StringToPass, ((dynamic)vm[1]).NumberToPass), commandId);

            var testObject = new
            {
                A = new[]
                {
                    new TestA() { StringToPass = "test" }
                },
                NumberToPass = 16
            };
            var viewRoot = new DotvvmView() { DataContext = testObject };
            viewRoot.SetBinding(Validate.TargetProperty, new ValueBindingExpression(vm => vm.Last(), "$root"));

            var placeholder = new HtmlGenericControl("div");
            placeholder.SetBinding(DotvvmBindableControl.DataContextProperty, path[0]);
            viewRoot.Children.Add(placeholder);

            var button = new Button();
            button.SetBinding(ButtonBase.ClickProperty, command);
            placeholder.Children.Add(button);

            var resolver = new CommandResolver();
            var context = new DotvvmRequestContext() { ViewModel = testObject };
            context.ModelState.ValidationTargetPath = KnockoutHelper.GetValidationTargetExpression(button);

            resolver.GetFunction(viewRoot, context, path.Select(v => v.Javascript).ToArray(), commandId).Action();

            Assert.AreEqual(testObject.NumberToPass, testObject.A[0].ResultInt);
            Assert.AreEqual(testObject.A[0].ResultString, testObject.A[0].ResultString);
        }

        //[TestMethod]
        //[ExpectedException(typeof(UnauthorizedAccessException))]
        //public void CommandResolver_CannotCallSetter()
        //{
        //    var testObject = new TestA()
        //    {
        //        StringToPass = "a"
        //    };
        //    var viewRoot = new DotvvmView() { DataContext = testObject };
        //    viewRoot.SetBinding(Validate.TargetProperty, new ValueBindingExpression("_root"));
        //    viewRoot.SetBinding(DotvvmProperty.Register<Action, DotvvmView>("Test"), new CommandBindingExpression("set_StringToPass(StringToPass)"));

        //    var path = new string[] { };
        //    var command = "set_StringToPass(StringToPass)";

        //    var resolver = new CommandResolver();
        //    var context = new DotvvmRequestContext() { ViewModel = testObject };
        //    resolver.GetFunction(viewRoot, context, path, command).GetAction()();
        //}

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
