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
        public void JsNullCheck_SimpleMemberAccess()
        {
            var expr =
                new JsIdentifierExpression("a").WithAnnotation(MayBeNullAnnotation.Instance)
                .Member("b")
                .Member("c");
            expr = JavascriptNullCheckAdder.AddNullChecks(expr);
            var node = JsTemporaryVariableResolver.ResolveVariables(expr);
            Assert.AreEqual("a==null?null:a.b.c", node.FormatScript(), node.FormatScript(niceMode: true));
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
            Assert.AreEqual("function(b){return (b=a==null?null:a.b)==null?null:b.c;}()", node.FormatScript(), node.FormatScript(niceMode: true));
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
            Assert.AreEqual("a&&a.b.c", node.FormatScript(), node.FormatScript(niceMode: true));
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
            Assert.AreEqual("(a&&a.b()||{}).c", node.FormatScript(), node.FormatScript(niceMode: true));
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
            Assert.AreEqual("function(b){return (b=a&&a.b())&&b.c();}()", node.FormatScript(), node.FormatScript(niceMode: true));
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
            Assert.AreEqual("(a&&a[5]||[])[7]", node.FormatScript(), node.FormatScript(niceMode: true));
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
            Assert.AreEqual("(a&&a(G)||eval)()", node.FormatScript(), node.FormatScript(niceMode: true));
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
            Assert.AreEqual("function(b){return (b=c?a:c2?a2:a3)==null?null:b.length;}()", node.FormatScript(), node.FormatScript(niceMode: true));
        }
    }
}
