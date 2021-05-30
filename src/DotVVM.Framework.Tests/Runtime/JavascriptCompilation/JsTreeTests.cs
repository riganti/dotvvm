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
            Assert.ThrowsException<InvalidOperationException>(() => left.MemberName = "omg");
            Assert.AreEqual(left.MemberName, "lol");
            Assert.ThrowsException<InvalidOperationException>(() => tree.Right.CastTo<JsLiteral>().Value = 8);
        }
    }
}
