using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class BindingExpressionBuilderTests
    {
        private DotvvmConfiguration configuration;
        private BindingTestHelper bindingHelper;
        private BindingCompilationService bindingService;

        [TestInitialize]
        public void Init()
        {
            this.configuration = DotvvmTestHelper.DefaultConfig;
            this.bindingHelper = new BindingTestHelper(configuration);
            this.bindingService = bindingHelper.BindingService;
        }

        [TestMethod]
        public void BindingExpressionBuilder_ConstructorCall_SimpleClass_Valid()
        {
            var context = DataContextStack.Create(typeof(TestViewModel));
            var imports = new[] { new NamespaceImport("System") };
            var expression = bindingHelper.ParseBinding("new DateTime(2023, 1, 1)", context, typeof(DateTime), imports);
            
            Assert.AreEqual(typeof(DateTime), expression.Type);
            Assert.IsInstanceOfType(expression, typeof(NewExpression));
            
            var newExpr = (NewExpression)expression;
            Assert.AreEqual(3, newExpr.Arguments.Count);
            Assert.AreEqual(typeof(DateTime).GetConstructor([ typeof(int), typeof(int), typeof(int) ]), newExpr.Constructor);
        }

        [TestMethod]
        public void BindingExpressionBuilder_ConstructorCall_WithArguments_Valid()
        {
            var context = DataContextStack.Create(typeof(TestViewModel));
            var imports = new[] { new NamespaceImport("System") };
            var expression = bindingHelper.ParseBinding("new TimeSpan(10000)", context, typeof(TimeSpan), imports);
            
            Assert.AreEqual(typeof(TimeSpan), expression.Type);
            Assert.IsInstanceOfType(expression, typeof(NewExpression));
            
            var newExpr = (NewExpression)expression;
            Assert.AreEqual(1, newExpr.Arguments.Count);
            Assert.AreEqual(typeof(TimeSpan).GetConstructor([ typeof(long) ]), newExpr.Constructor);
        }

        [TestMethod]
        public void BindingExpressionBuilder_ConstructorCall_OverloadResolution_Valid()
        {
            var context = DataContextStack.Create(typeof(TestViewModel));
            var imports = new[] { new NamespaceImport("System") };
            var expression = bindingHelper.ParseBinding("new TimeSpan(1, 2, Byte)", context, typeof(TimeSpan), imports);
            
            Assert.AreEqual(typeof(TimeSpan), expression.Type);
            Assert.IsInstanceOfType(expression, typeof(NewExpression));
            
            var newExpr = (NewExpression)expression;
            Assert.AreEqual(3, newExpr.Arguments.Count);
            Assert.AreEqual(typeof(TimeSpan).GetConstructor([ typeof(int), typeof(int), typeof(int) ]), newExpr.Constructor);
        }

        [TestMethod]
        public void BindingExpressionBuilder_TypeInferredConstructorCall_FromExpectedType_Valid()
        {
            var context = DataContextStack.Create(typeof(TestViewModel));
            var imports = new[] { new NamespaceImport("System") };
            var expression = bindingHelper.ParseBinding("new(2023, 1, Byte)", context, typeof(DateTime), imports);
            
            Assert.AreEqual(typeof(DateTime), expression.Type);
            Assert.IsInstanceOfType(expression, typeof(NewExpression));
            
            var newExpr = (NewExpression)expression;
            Assert.AreEqual(3, newExpr.Arguments.Count);
            Assert.AreEqual(typeof(DateTime).GetConstructor([typeof(int), typeof(int), typeof(int)]), newExpr.Constructor);
        }

        [TestMethod]
        public void BindingExpressionBuilder_ArrayConstruction_WithSize_Valid()
        {
            var context = DataContextStack.Create(typeof(TestViewModel));
            var expression = bindingHelper.ParseBinding("new int[5]", context, typeof(int[]));
            
            Assert.AreEqual(typeof(int[]), expression.Type);
            Assert.IsInstanceOfType(expression, typeof(NewArrayExpression));
            
            var arrayExpr = (NewArrayExpression)expression;
            Assert.AreEqual(ExpressionType.NewArrayBounds, arrayExpr.NodeType);
            Assert.AreEqual(1, arrayExpr.Expressions.Count);
            Assert.AreEqual(typeof(int), arrayExpr.Type.GetElementType());
        }

        [TestMethod]
        public void BindingExpressionBuilder_ArrayConstruction_WithInitializer_Valid()
        {
            var context = DataContextStack.Create(typeof(TestViewModel));
            var expression = bindingHelper.ParseBinding("new int[] { 1, 2, 3 }", context, typeof(int[]));
            
            Assert.AreEqual(typeof(int[]), expression.Type);
            Assert.IsInstanceOfType(expression, typeof(NewArrayExpression));
            
            var arrayExpr = (NewArrayExpression)expression;
            Assert.AreEqual(ExpressionType.NewArrayInit, arrayExpr.NodeType);
            Assert.AreEqual(3, arrayExpr.Expressions.Count);
            Assert.AreEqual(typeof(int), arrayExpr.Type.GetElementType());
        }

        [TestMethod]
        public void BindingExpressionBuilder_ArrayConstruction_TypeInferred_Valid()
        {
            var context = DataContextStack.Create(typeof(TestViewModel));
            var expression = bindingHelper.ParseBinding("new[] { 1, 2, 3 }", context, typeof(int[]));
            
            Assert.AreEqual(typeof(int[]), expression.Type);
        }

        [TestMethod]
        public void BindingExpressionBuilder_ArrayConstruction_MixedTypes_Valid()
        {
            var context = DataContextStack.Create(typeof(TestViewModel));
            var expression = bindingHelper.ParseBinding("new[] { 1, 1.2 }", context);
            
            Assert.AreEqual(typeof(double[]), ((UnaryExpression)expression).Operand.Type);
        }

        [TestMethod]
        public void BindingExpressionBuilder_ArrayConstruction_WithVariableSize_Valid()
        {
            var context = DataContextStack.Create(typeof(TestViewModel));
            var expression = bindingHelper.ParseBinding("new string[Size]", context, typeof(string[]));

            Assert.AreEqual(typeof(string[]), expression.Type);
            Assert.IsInstanceOfType(expression, typeof(NewArrayExpression));

            var arrayExpr = (NewArrayExpression)expression;
            Assert.AreEqual(ExpressionType.NewArrayBounds, arrayExpr.NodeType);
            Assert.AreEqual(typeof(string), arrayExpr.Type.GetElementType());
            var member = XAssert.IsAssignableFrom<MemberExpression>(XAssert.Single(arrayExpr.Expressions));
            Assert.AreEqual("Size", member.Member.Name);
            Assert.AreEqual(typeof(TestViewModel), member.Member.DeclaringType);
            Assert.IsInstanceOfType(member.Expression, typeof(ParameterExpression));
        }

        [TestMethod]
        public void BindingExpressionBuilder_NestedConstructorCalls_Valid()
        {
            var context = DataContextStack.Create(typeof(TestViewModel));
            var imports = new[] { new NamespaceImport("System") };
            var expression = bindingHelper.ParseBinding("new TimeSpan(new DateTime(2023, 1, 1).Ticks)", context, typeof(TimeSpan), imports);

            Assert.AreEqual(typeof(TimeSpan), expression.Type);
            Assert.IsInstanceOfType(expression, typeof(NewExpression));
        }

        [TestMethod]
        public void BindingExpressionBuilder_ConstructorCall_GenericType_Valid()
        {
            var context = DataContextStack.Create(typeof(TestViewModel));
            var imports = new[] { new NamespaceImport("System") };
            var expression = bindingHelper.ParseBinding("new DateTime()", context, typeof(DateTime), imports);
            
            Assert.AreEqual(typeof(DateTime), expression.Type);
            
            // For struct parameterless constructor, we expect Expression.Default
            Assert.IsInstanceOfType(expression, typeof(DefaultExpression));
        }

        [TestMethod]
        public void BindingExpressionBuilder_ConstructorCall_InvalidType_ThrowsException()
        {
            var context = DataContextStack.Create(typeof(TestViewModel));
            var exception = XAssert.ThrowsAny<Exception>(() => bindingHelper.ParseBinding("new NonExistentType()", context, typeof(object)));
            Assert.AreEqual("Could not resolve identifier 'NonExistentType'.", exception.Message);
        }

        [TestMethod]
        public void BindingExpressionBuilder_ConstructorCall_NoMatchingConstructor_ThrowsException()
        {
            var context = DataContextStack.Create(typeof(TestViewModel));
            var imports = new[] { new NamespaceImport("System") };
            var exception = Assert.ThrowsException<BindingCompilationException>(() => bindingHelper.ParseBinding("new DateTime(\"invalid\").AddDays(0.1)", context, null, imports));
            Assert.AreEqual("Could not find a constructor for type 'System.DateTime(string)'.", exception.InnerException.Message);
            Assert.AreEqual("\"invalid\"", exception.Tokens[^2].Text);
        }

        [TestMethod]
        public void BindingExpressionBuilder_TypeInferredConstructorCall_CannotInfer_ThrowsException()
        {
            var context = DataContextStack.Create(typeof(TestViewModel));
            var exception = Assert.ThrowsException<BindingCompilationException>(() => bindingHelper.ParseBinding("var a = new(1, 2, 3); a + 12", context, typeof(object)));
            Assert.AreEqual("Could not infer the constructed type of new (1, 2, 3). Please specify the type name explicitly.", exception.Message);
        }

        [TestMethod]
        public void BindingExpressionBuilder_ArrayConstruction_InvalidSize_ThrowsException()
        {
            var context = DataContextStack.Create(typeof(TestViewModel));
            var exception = XAssert.ThrowsAny<Exception>(() => bindingHelper.ParseBinding("new int[\"invalid\"]", context, typeof(int[])));
            Assert.AreEqual("Cannot convert 'invalid' of type string to System.Int64.", exception.Message);
        }

        public class TestViewModel
        {
            public string Name { get; set; } = "";
            public int Count { get; set; }
            public List<string> Items { get; set; } = new List<string>();
            public int Size { get; set; } = 5;
            public byte Byte { get; } = 12;
        }
    }
}
