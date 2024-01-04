using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests
{
    public static class AssertEx
    {
        public static void AssertNode(BindingParserNode node, string expectedDisplayString, int start, int length, bool hasErrors = false)
        {
            Assert.AreEqual(expectedDisplayString, node.ToDisplayString(), $"Node {node.GetType().Name}: display string incorrect.");
            Assert.AreEqual(start, node.StartPosition, $"Node {node.GetType().Name}: Start position incorrect.");
            Assert.AreEqual(length, node.Length, $"Node {node.GetType().Name}: Length incorrect.");

            if (hasErrors)
            {
                Assert.IsTrue(node.HasNodeErrors);
            }
            else
            {
                Assert.IsFalse(node.HasNodeErrors);
            }
        }
    }
}
