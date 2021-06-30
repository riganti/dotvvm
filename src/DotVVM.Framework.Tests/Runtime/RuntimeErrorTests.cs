using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.ResourceManagement.ClientGlobalize;
using System.Globalization;
using DotVVM.Framework.Compilation.Parser;
using System.Reflection;
using DotVVM.Framework.Binding;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using CheckTestOutput;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Compilation;

namespace DotVVM.Framework.Tests.Runtime
{
    static class CheckExtensions
    {
        static string FormatException(Exception exception)
        {
            var topFrame = new EnhancedStackTrace(exception).FirstOrDefault(m => !m.MethodInfo.DeclaringType.Namespace.StartsWith("DotVVM.Framework.Binding"))?.MethodInfo.ToString();
            var msg = $"{exception.GetType().Name} occurred: {exception.Message}";
            if (topFrame != null) msg += $"\n    at {topFrame}";
            if (exception is AggregateException aggregateException && aggregateException.InnerExceptions.Count > 1)
            {
                var inner = aggregateException.InnerExceptions.Select(FormatException).StringJoin("\n\n");
                return $"{msg}\n\n{inner}";
            }
            else
            {
                var inner = exception.InnerException?.Apply(FormatException);

                if (inner == null)
                    return msg;
                else
                    return $"{msg}\n\n{inner}";
            }
        }

        public static void CheckException(this OutputChecker check, Action action, string checkName = null, string fileExtension = "txt", [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                var error = FormatException(exception);
                check.CheckString(error, checkName, fileExtension, memberName, sourceFilePath);
                return;
            }
            throw new Exception("Expected test to fail.");
        }
    }

    [TestClass]
    public class RuntimeErrorTests
    {
        OutputChecker check = new OutputChecker("testoutputs");
        DotvvmConfiguration config = DotvvmTestHelper.DefaultConfig;
        BindingCompilationService bindingService => config.ServiceProvider.GetService<BindingCompilationService>();

        [TestMethod]
        [Ignore("Doesn't work on Windows because of different EOL sequences.")]
        public void NonExistentBindingProperty()
        {
            var binding = ValueBindingExpression.CreateBinding(bindingService, a => 12, DataContextStack.Create(typeof(string)));

            check.CheckException(() => binding.GetProperty<OriginalStringBindingProperty>());
        }

        [TestMethod]
        [Ignore("Doesn't work on Windows because of different EOL sequences.")]
        public void PropertyResolverFailure()
        {
            var binding = ValueBindingExpression.CreateBinding(bindingService.WithoutInitialization(), a => config.ToString(), DataContextStack.Create(typeof(string)));
            //                                                                ^ disables initialization check

            check.CheckException(() => binding.GetProperty<KnockoutExpressionBindingProperty>());
        }

        [TestMethod]
        [Ignore("Doesn't work on Windows because of different EOL sequences.")]
        public void InitResolverFailure()
        {

            check.CheckException(() => ValueBindingExpression.CreateBinding(bindingService, a => config.ToString(), DataContextStack.Create(typeof(string))));
        }

        [TestMethod]
        [Ignore("Doesn't work on Windows because of different EOL sequences.")]
        public void DataContextStack_ToString()
        {
            var parent = DataContextStack.Create(typeof(string), DataContextStack.Create(typeof(RuntimeErrorTests)));
            var d = DataContextStack.Create(
                typeof(int),
                parent,
                imports: new [] { new NamespaceImport("System"), new NamespaceImport("System.Text", "Text") },
                extensionParameters: new [] { new CurrentCollectionIndexExtensionParameter() });

            check.CheckString(d.ToString());
        }

        [TestMethod]
        [Ignore("Doesn't work at all.")]
        public void CantFindDataContextSpace()
        {
            var binding = ValueBindingExpression.CreateBinding(bindingService, a => false, DataContextStack.Create(typeof(string)));
            var control = new HtmlGenericControl("div");
            control.SetValue(Internal.DataContextTypeProperty, DataContextStack.Create(typeof(string), DataContextStack.Create(typeof(int))));
            control.SetProperty(c => c.Visible, binding);
            check.CheckException(() => control.Visible.ToString());
        }
    }
}
