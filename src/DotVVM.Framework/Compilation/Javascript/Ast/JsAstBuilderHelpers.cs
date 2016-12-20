using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public static class JsAstBuilderHelpers
    {
        public static JsExpression Member(this JsExpression target, string memberName)
        {
            if (target == null) return new JsIdentifierExpression(memberName);
            else return new JsMemberAccessExpression(target, memberName);
        }

        public static JsExpression Invoke(this JsExpression target, params JsExpression[] arguments)
        {
            return new JsInvocationExpression(target, arguments);
        }

        public static JsExpression Indexer(this JsExpression target, JsExpression argument)
        {
            return new JsIndexerExpression(target, argument);
        }

        public static string FormatScript(this JsNode node, bool niceMode = false, string indent = "\t")
        {
            var visitor = new JsFormattingVisitor(niceMode, indent);
            node.AcceptVisitor(visitor);
            return visitor.ToString();
        }
    }
}
