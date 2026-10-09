using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Compilation.Javascript;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime.JavascriptCompilation
{
    [TestClass]
    public class JsNullChecksTests
    {
        [TestMethod]
        [DataRow(false, "lookup()?.length")]
        [DataRow(true, "lookup?.()?.length")]
        public void JsNullCheck_ObservableItselfMayBeMissing(bool missingObservable, string expected)
        {
            var observable = new JsIdentifierExpression("lookup")
                .WithAnnotation(ResultIsObservableAnnotation.Instance)
                .WithAnnotation(MayBeNullAnnotation.Instance)
                .WithAnnotation(new ViewModelInfoAnnotation(typeof(string)))
                .WithConditionalAnnotation(missingObservable, ObservableMayBeNullAnnotation.Instance);
            var expression = new JsParenthesizedExpression(observable.Member("length"));

            expression.AcceptVisitor(new KnockoutObservableHandlingVisitor(allowObservableResult: false));
            JavascriptNullCheckAdder.AddNullChecks(expression);
            var result = JsTemporaryVariableResolver.ResolveVariables(expression.Expression.Detach());

            Assert.AreEqual(expected, result.FormatScript());
        }

        [TestMethod]
        public void JsNullCheck_SimpleMemberAccess()
        {
            var expr =
                new JsIdentifierExpression("a").WithAnnotation(MayBeNullAnnotation.Instance)
                .Member("b")
                .Member("c");
            expr = JavascriptNullCheckAdder.AddNullChecks(expr);
            var node = JsTemporaryVariableResolver.ResolveVariables(expr);
            Assert.AreEqual("a?.b.c", node.FormatScript(), node.FormatScript(niceMode: true));
        }

        [TestMethod]
        public void JsNullCheck_SimpleMemberAccess2()
        {
            var expr =
                new JsIdentifierExpression("a").WithAnnotation(MayBeNullAnnotation.Instance)
                .Member("b").WithAnnotation(MayBeNullAnnotation.Instance)
                .Member("c");
            expr = JavascriptNullCheckAdder.AddNullChecks(expr);
            var node = JsTemporaryVariableResolver.ResolveVariables(expr);
            Assert.AreEqual("a?.b?.c", node.FormatScript(), node.FormatScript(niceMode: true));
        }

        [TestMethod]
        public void JsNullCheck_SimpleObjectMemberAccess()
        {
            var expr =
                new JsIdentifierExpression("a").WithAnnotation(MayBeNullAnnotation.Instance).WithAnnotation(new ViewModelInfoAnnotation(typeof(JsNullChecksTests)))
                .Member("b")
                .Member("c");
            expr = JavascriptNullCheckAdder.AddNullChecks(expr);
            var node = JsTemporaryVariableResolver.ResolveVariables(expr);
            Assert.AreEqual("a?.b.c", node.FormatScript(), node.FormatScript(niceMode: true));
        }

        [TestMethod]
        public void JsNullCheck_SimpleObjectMemberAccess2()
        {
            var expr =
                new JsIdentifierExpression("a").WithAnnotation(MayBeNullAnnotation.Instance).WithAnnotation(new ViewModelInfoAnnotation(typeof(JsNullChecksTests)))
                .Member("b").Invoke().WithAnnotation(MayBeNullAnnotation.Instance).WithAnnotation(new ViewModelInfoAnnotation(typeof(JsNullChecksTests)))
                .Member("c");
            expr = JavascriptNullCheckAdder.AddNullChecks(expr);
            var node = JsTemporaryVariableResolver.ResolveVariables(expr);
            Assert.AreEqual("a?.b()?.c", node.FormatScript(), node.FormatScript(niceMode: true));
        }

        [TestMethod]
        public void JsNullCheck_SimpleObjectMemberAccess3()
        {
            var expr =
                new JsIdentifierExpression("a").WithAnnotation(MayBeNullAnnotation.Instance).WithAnnotation(new ViewModelInfoAnnotation(typeof(JsNullChecksTests)))
                .Member("b").Invoke().WithAnnotation(MayBeNullAnnotation.Instance).WithAnnotation(new ViewModelInfoAnnotation(typeof(JsNullChecksTests)))
                .Member("c").Invoke();
            expr = JavascriptNullCheckAdder.AddNullChecks(expr);
            var node = JsTemporaryVariableResolver.ResolveVariables(expr);
            Assert.AreEqual("a?.b()?.c()", node.FormatScript(), node.FormatScript(niceMode: true));
        }

        [TestMethod]
        public void JsNullCheck_SimpleObjectIndexer()
        {
            var expr =
                new JsIdentifierExpression("a").WithAnnotation(MayBeNullAnnotation.Instance).WithAnnotation(new ViewModelInfoAnnotation(typeof(JsNullChecksTests)))
                .Indexer(new JsLiteral(5)).WithAnnotation(MayBeNullAnnotation.Instance).WithAnnotation(new ViewModelInfoAnnotation(typeof(JsNullChecksTests)))
                .Indexer(new JsLiteral(7));
            expr = JavascriptNullCheckAdder.AddNullChecks(expr);
            var node = JsTemporaryVariableResolver.ResolveVariables(expr);
            Assert.AreEqual("a?.[5]?.[7]", node.FormatScript(), node.FormatScript(niceMode: true));
        }

        [TestMethod]
        public void JsNullCheck_SimpleObjectInvocation()
        {
            var expr =
                new JsIdentifierExpression("a").WithAnnotation(MayBeNullAnnotation.Instance).WithAnnotation(new ViewModelInfoAnnotation(typeof(JsNullChecksTests)))
                .Invoke(new JsIdentifierExpression("G")).WithAnnotation(MayBeNullAnnotation.Instance).WithAnnotation(new ViewModelInfoAnnotation(typeof(JsNullChecksTests)))
                .Invoke();
            expr = JavascriptNullCheckAdder.AddNullChecks(expr);
            var node = JsTemporaryVariableResolver.ResolveVariables(expr);
            Assert.AreEqual("a?.(G)?.()", node.FormatScript(), node.FormatScript(niceMode: true));
        }

        [TestMethod]
        public void JsNullCheck_InvocationPreservesReceiver()
        {
            var expr = new JsIdentifierExpression("getObject").Invoke()
                .Member("method").WithAnnotation(MayBeNullAnnotation.Instance)
                .Invoke(new JsIdentifierExpression("getArgument").Invoke());

            expr = JavascriptNullCheckAdder.AddNullChecks(expr);
            var node = JsTemporaryVariableResolver.ResolveVariables(expr);

            Assert.AreEqual("getObject().method?.(getArgument())", node.FormatScript());
        }

        [TestMethod]
        public void JsNullCheck_IndexerWithSideEffects()
        {
            var expr = new JsIdentifierExpression("getArray").Invoke()
                .WithAnnotation(MayBeNullAnnotation.Instance)
                .Indexer(new JsIdentifierExpression("getIndex").Invoke())
                .Member("value");

            expr = JavascriptNullCheckAdder.AddNullChecks(expr);
            var node = JsTemporaryVariableResolver.ResolveVariables(expr);

            Assert.AreEqual("getArray()?.[getIndex()].value", node.FormatScript());
        }

        [TestMethod]
        public void NestedConditionals()
        {
            JsExpression expr =
                new JsMemberAccessExpression(
                    new JsConditionalExpression(
                        new JsIdentifierExpression("c"),
                        new JsIdentifierExpression("a"),
                        new JsConditionalExpression(
                            new JsIdentifierExpression("c2"),
                            new JsIdentifierExpression("a2").WithAnnotation(MayBeNullAnnotation.Instance),
                            new JsIdentifierExpression("a3"))),
                "length");

            expr = JavascriptNullCheckAdder.AddNullChecks(expr);
            var node = JsTemporaryVariableResolver.ResolveVariables(expr);
            Assert.AreEqual("(c?a:c2?a2:a3)?.length", node.FormatScript(), node.FormatScript(niceMode: true));
        }
    }
}
