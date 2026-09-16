using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Tests.Runtime.JavascriptCompilation
{
    [TestClass]
    public class JsTreeTests
    {
        [TestMethod]
        public void JsTree_FrozenBlocksMutations()
        {
            var tree = new JsBinaryExpression(new JsIdentifierExpression("a").Member("b"), BinaryOperatorType.Plus, new JsLiteral(5));
            var left = tree.Left.CastTo<JsMemberAccessExpression>();
            left.MemberName = "lol";
            tree.Freeze();
            Assert.ThrowsException<Configuration.FreezableUtils.ObjectIsFrozenException>(() => left.MemberName = "omg");
            Assert.AreEqual(left.MemberName, "lol");
            Assert.ThrowsException<Configuration.FreezableUtils.ObjectIsFrozenException>(() => tree.Right.CastTo<JsLiteral>().Value = 8);
        }

        [TestMethod]
        public void JsTree_OptionalIndexerCloningAndFreezing()
        {
            var indexer = new JsIndexerExpression(new JsIdentifierExpression("a"), new JsLiteral(0));
            Assert.IsFalse(indexer.IsOptional);
            indexer.IsOptional = true;
            indexer.Freeze();
            Assert.ThrowsException<Configuration.FreezableUtils.ObjectIsFrozenException>(() => indexer.IsOptional = false);

            var clone = indexer.Clone();
            Assert.IsTrue(clone.IsOptional);
            clone.IsOptional = false;
            Assert.IsTrue(indexer.IsOptional);
            Assert.AreEqual("a[0]", clone.FormatScript());
        }

        [TestMethod]
        public void JsTree_OptionalInvocationCloningAndFreezing()
        {
            var invocation = new JsInvocationExpression(new JsIdentifierExpression("a"));
            Assert.IsFalse(invocation.IsOptional);
            invocation.IsOptional = true;
            invocation.Freeze();
            Assert.ThrowsException<Configuration.FreezableUtils.ObjectIsFrozenException>(() => invocation.IsOptional = false);

            var clone = invocation.Clone();
            Assert.IsTrue(clone.IsOptional);
            clone.IsOptional = false;
            Assert.IsTrue(invocation.IsOptional);
            Assert.AreEqual("a()", clone.FormatScript());
        }

        [TestMethod]
        public void TemporaryVariableResolver_DoesNotCollideWithArrowParameter()
        {
            var temporary = new JsTemporaryVariableParameter(allowInlining: false);
            var body = new JsBinaryExpression(temporary.ToExpression(), BinaryOperatorType.Sequence, temporary.ToExpression());
            var expression = new JsArrowFunctionExpression(new[] { new JsIdentifier("a") }, body);

            var resolved = JsTemporaryVariableResolver.ResolveVariables(expression);

            Assert.AreEqual("(a)=>{let b;return b,b;}", resolved.FormatScript());
        }

        [TestMethod]
        public void AssignParameters_ReplacesRootParameter()
        {
            var parameter = new CodeSymbolicParameter("root");
            JsNode expression = parameter.ToExpression();

            expression = expression.AssignParameters(p => p == parameter ? new JsIdentifierExpression("x") : null);

            Assert.AreEqual("x", expression.FormatScript());
        }
    }
}
