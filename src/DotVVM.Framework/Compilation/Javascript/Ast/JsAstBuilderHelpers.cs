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
            node.FixParenthesis();
            var visitor = new JsFormattingVisitor(niceMode, indent);
            node.AcceptVisitor(visitor);
            return visitor.GetParameterlessResult();
        }

        public static ParametrizedCode FormatParametrizedScript(this JsNode node, bool niceMode = false, string indent = "\t")
        {
            node.FixParenthesis();
            var visitor = new JsFormattingVisitor(niceMode, indent);
            node.AcceptVisitor(visitor);
            return visitor.GetResult();
        }

        public static JsNode FixParenthesis(this JsNode node)
        {
            var visitor = new JsParensFixingVisitor();
            node.AcceptVisitor(visitor);
            return node;
        }
    }
}
