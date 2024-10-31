using DotVVM.Framework.Compilation.Javascript.Ast;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DotVVM.Framework.Compilation.Javascript;

namespace DotVVM.Framework.Tests.Runtime.JavascriptCompilation
{
    [TestClass]
    public class JsParametrizedCodeTests
    {
        static CodeSymbolicParameter symbolA = new CodeSymbolicParameter("A");
        static CodeSymbolicParameter symbolB = new CodeSymbolicParameter("B");
        static CodeSymbolicParameter symbolC = new CodeSymbolicParameter("C", defaultAssignment: new JsIdentifier("defaultValue").FormatParametrizedScript());
        static CodeSymbolicParameter symbolD = new CodeSymbolicParameter("D", defaultAssignment: new JsBinaryExpression(new JsSymbolicParameter(symbolA), BinaryOperatorType.Times, new JsSymbolicParameter(symbolC)).FormatParametrizedScript());
        static CodeSymbolicParameter symbolE = new CodeSymbolicParameter("E", defaultAssignment: new JsBinaryExpression(new JsSymbolicParameter(symbolA), BinaryOperatorType.Plus, new JsSymbolicParameter(symbolD)).FormatParametrizedScript());

        [TestMethod]
        public void SymbolicParameters_Global()
        {
            // full would be "global.a+global
            var pcode = new JsBinaryExpression(new JsMemberAccessExpression(new JsSymbolicParameter(symbolA), "a"), BinaryOperatorType.Plus,
                    new JsSymbolicParameter(symbolA))
                .FormatParametrizedScript();
            Assert.AreEqual("a+global",
                pcode.ToString(o => o == symbolA ? CodeParameterAssignment.FromExpression(new JsIdentifierExpression("global"), isGlobalContext: true) :
                                    throw new Exception()));
            Assert.AreEqual("a+global",
                pcode.AssignParameters(o => o == symbolA ? CodeParameterAssignment.FromExpression(new JsIdentifierExpression("global"), isGlobalContext: true) :
                                            throw new Exception())
                    .ToString(o => default));
        }

        [TestMethod]
        public void SymbolicParameters_Global_ThroughDefault()
        {
            var globalDefaultSymbol = new CodeSymbolicParameter("global", defaultAssignment: symbolA.ToParametrizedCode());
            // may be "global.a+global" or "a+global" - doesn't matter if the optimization works in this case, but it shouldn't be broken
            var pcode = new JsBinaryExpression(new JsMemberAccessExpression(new JsSymbolicParameter(globalDefaultSymbol), "a"), BinaryOperatorType.Plus,
                                new JsSymbolicParameter(globalDefaultSymbol))
                            .FormatParametrizedScript();
            Assert.AreEqual("global.a+global",
                pcode.ToString(o => o == symbolA ? CodeParameterAssignment.FromExpression(new JsIdentifierExpression("global"), isGlobalContext: true) :
                                    default));
            Assert.AreEqual("global.a+global",
                pcode.AssignParameters(o => o == symbolA ? CodeParameterAssignment.FromExpression(new JsIdentifierExpression("global"), isGlobalContext: true) :
                                            default)
                    .ToString(o => default));
        }

        [TestMethod]
        public void SymbolicParameters_Throws_Unassigned()
        {
            var pcode = new JsBinaryExpression(new JsMemberAccessExpression(new JsSymbolicParameter(symbolA), "a"), BinaryOperatorType.Plus, new JsSymbolicParameter(symbolA)).FormatParametrizedScript();
            
            var pcode2 = pcode.AssignParameters(o => default); // fine, not all have to be assigned
            var pcodeResolved = pcode.AssignParameters(o => o == symbolA ? CodeParameterAssignment.FromIdentifier("x") : default);

            Assert.AreEqual("y.a+y", pcode.ToString(o => o == symbolA ? CodeParameterAssignment.FromIdentifier("y") : default));
            Assert.AreEqual("z.a+z", pcode2.ToString(o => o == symbolA ? CodeParameterAssignment.FromIdentifier("z") : default));
            Assert.AreEqual("x.a+x", pcodeResolved.ToString(o => default));
            Assert.AreEqual("x.a+x", pcodeResolved.ToString(o => o == symbolA ? CodeParameterAssignment.FromIdentifier("AREADY_ASSIGNED_BEFORE") : default));

            var ex = Assert.ThrowsException<InvalidOperationException>(() => pcode.ToString(o => default));
            XAssert.Contains("Assignment of parameter '{A|", ex.Message);

            ex = Assert.ThrowsException<InvalidOperationException>(() => pcode2.ToString(o => default));
            XAssert.Contains("Assignment of parameter '{A|", ex.Message);
        }

        [TestMethod]
        public void SymbolicParameters_Partial_DefaultChain()
        {
            var expr = new JsBinaryExpression(symbolA.ToExpression(), BinaryOperatorType.Plus, symbolE.ToExpression());
            var pcode = expr.FormatParametrizedScript();

            Assert.AreEqual("x+(x+x*defaultValue)", pcode.ToString(o => o == symbolA ? CodeParameterAssignment.FromIdentifier("x") : default));
            Assert.AreEqual("y+(y+y*defaultValue)", pcode.ToString(o => o == symbolA ? CodeParameterAssignment.FromIdentifier("y") : default));

            // substitute symbolD for identifier, order doesn't matter
            Assert.AreEqual("x+(x+D)",
                pcode.AssignParameters(o => o == symbolA ? CodeParameterAssignment.FromIdentifier("x") : default)
                     .ToString(o => o == symbolD ? CodeParameterAssignment.FromExpression(new JsIdentifierExpression("D")) : default));
            Assert.AreEqual("x+(x+D)",
                pcode.AssignParameters(o => o == symbolD ? CodeParameterAssignment.FromExpression(new JsIdentifierExpression("D")) : default)
                     .ToString(o => o == symbolA ? CodeParameterAssignment.FromIdentifier("x") : default));

            // substitute symbolD for symbolA, order now matters
            Assert.AreEqual("x+(x+x)",
                pcode.AssignParameters(o => o == symbolD ? CodeParameterAssignment.FromExpression(new JsSymbolicParameter(symbolA)) : default)
                     .ToString(o => o == symbolA ? CodeParameterAssignment.FromIdentifier("x") : default));
            Assert.AreEqual("x+(x+y)",
                pcode.AssignParameters(o => o == symbolA ? CodeParameterAssignment.FromIdentifier("x") : default)
                     .AssignParameters(o => o == symbolD ? CodeParameterAssignment.FromExpression(new JsSymbolicParameter(symbolA)) : default)
                     .ToString(o => o == symbolA ? CodeParameterAssignment.FromIdentifier("y") : default));
        }

        [TestMethod]
        public void SymbolicParameters_AddDefault()
        {
            var expr = new JsBinaryExpression(symbolA.ToExpression(), BinaryOperatorType.Plus, symbolE.ToExpression());
            var pcode = expr.FormatParametrizedScript();

            // add default for symbolA
            var assigned = pcode.AssignParameters(o => o == symbolA ? symbolA.ToExpression(CodeParameterAssignment.FromIdentifier("a")).FormatParametrizedScript() : default);

            Assert.AreEqual("a+(a+a*defaultValue)", assigned.ToString(o => default));
            Assert.AreEqual("a+(a+a*defaultValue)", assigned.ToDefaultString());

            Assert.AreEqual("x+(x+x*defaultValue)", assigned.ToString(o => o == symbolA ? CodeParameterAssignment.FromIdentifier("x") : default));
            Assert.AreEqual("a+x", assigned.ToString(o => o == symbolE ? CodeParameterAssignment.FromIdentifier("x") : default));
        }

#if DotNetCore
        [TestMethod]
        public void SymbolicParameters_NoAssignmentNoAllocation()
        {
            var expr = new JsBinaryExpression(symbolA.ToExpression(), BinaryOperatorType.Plus, symbolE.ToExpression());
            var pcode = expr.FormatParametrizedScript();
            Func<CodeSymbolicParameter, CodeParameterAssignment> noAssignment = o => default;

            var b = GC.GetAllocatedBytesForCurrentThread();
            pcode.AssignParameters(noAssignment);
            Assert.AreEqual(0, GC.GetAllocatedBytesForCurrentThread() - b);
        }
#endif
    }
}
