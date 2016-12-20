using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class JsFormattingVisitor : IJsNodeVisitor
    {
        public bool NiceMode { get; }
        public string IndentString { get; }

        public JsFormattingVisitor(bool niceMode = false, string indent = "\t")
        {
            this.NiceMode = niceMode;
            this.IndentString = indent;
        }

        StringBuilder result = new StringBuilder();
        protected void Emit(string str)
        {
            Debug.Assert(!str.Contains("\n"));
            result.Append(str);
        }

        protected void CommitLine()
        {
            if (NiceMode) {
                for (int i = 0; i < indentLevel; i++) {
                    result.Append(IndentString);
                }
            }
        }

        protected void OptionalSpace()
        {
            if (NiceMode) Emit(" ");
        }

        protected void SpaceForOperator(string op)
        {
            if (op.Any(char.IsLetter)) Emit(" ");
            else OptionalSpace();
        }

        protected void EmitOperator(string op)
        {
            SpaceForOperator(op);
            Emit(op);
            SpaceForOperator(op);
        }

        List<(int position, float priority)> possibleLineBreaks;
        int indentLevel = 0;
        protected void Indent()
        {
            indentLevel++;
        }
        protected void Dedent()
        {
            indentLevel--;
        }

        public override string ToString()
        {
            return result.ToString();
        }

        protected void VisitChildren(JsNode node)
        {
            foreach (var c in node.Children) {
                c.AcceptVisitor(this);
            }
        }

        public void VisitBinaryExpression(JsBinaryExpression binaryExpression)
        {
            binaryExpression.Left.AcceptVisitor(this);
            var op = binaryExpression.OperatorString;
            EmitOperator(op);
            binaryExpression.Right.AcceptVisitor(this);
        }

        public void VisitConditionalExpression(JsConditionalExpression conditionalExpression)
        {
            conditionalExpression.Condition.AcceptVisitor(this);
            EmitOperator("?");
            conditionalExpression.TrueExpression.AcceptVisitor(this);
            EmitOperator(":");
            conditionalExpression.FalseExpression.AcceptVisitor(this);
        }

        public void VisitIdentifier(JsIdentifier identifier)
        {
            Emit(identifier.Name);
        }

        public void VisitMemberAccessExpression(JsMemberAccessExpression memberAccessExpression)
        {
            memberAccessExpression.Target.AcceptVisitor(this);
            Emit(".");
            memberAccessExpression.MemberNameToken.AcceptVisitor(this);
        }

        public void VisitIdentifierExpression(JsIdentifierExpression identifierExpression)
        {
            identifierExpression.IdentifierToken.AcceptVisitor(this);
        }

        public void VisitInvocationExpression(JsInvocationExpression invocationExpression)
        {
            invocationExpression.Target.AcceptVisitor(this);
            Emit("(");
            int i = 0;
            foreach (var arg in invocationExpression.Arguments) {
                if (i++ > 0) { Emit(","); OptionalSpace(); }
                arg.AcceptVisitor(this);
            }
            Emit(")");
        }

        public void VisitParenthesizedExpression(JsParenthesizedExpression parenthesizedExpression)
        {
            Emit("(");
            parenthesizedExpression.Expression.AcceptVisitor(this);
            Emit(")");
        }

        public void VisitUnaryExpression(JsUnaryExpression unaryExpression)
        {
            EmitOperator(unaryExpression.OperatorString);
        }

        public void VisitIndexerExpression(JsIndexerExpression indexerExpression)
        {
            indexerExpression.Target.AcceptVisitor(this);
            Emit("[");
            indexerExpression.Argument.AcceptVisitor(this);
            Emit("]");
        }

        public void VisitLiteral(JsLiteral jsLiteral)
        {
            Emit(jsLiteral.LiteralValue);
        }
    }
}
