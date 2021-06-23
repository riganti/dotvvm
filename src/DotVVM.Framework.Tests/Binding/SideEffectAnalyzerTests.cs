using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class SideEffectAnalyzerTests
    {
        [TestMethod]
        public void SideEffectAnalyzer_AssignmentExpression()
        {
            var mutations = SideEffectAnalyzer.GetPossibleMutations(
                new JsBinaryExpression(
                    new JsAssignmentExpression(new JsIdentifierExpression("a"), new JsIdentifierExpression("b")),
                    BinaryOperatorType.Plus,
                    new JsAssignmentExpression(new JsIdentifierExpression("a").Member("kokos"), new JsLiteral(1))
                )
            );
            // `b` is assigned to `a`, so `a` is mutated
            Assert.IsTrue(mutations.MayMutate(new [] { "a" }));
            // `b` is not mutated
            Assert.IsFalse(mutations.MayMutate(new [] { "b" }));
            // `b` is assigned to `a` and `a.kokos` is mutated
            Assert.IsTrue(mutations.MayMutate(new [] { "b", "kokos" }));
        }
    }
}
