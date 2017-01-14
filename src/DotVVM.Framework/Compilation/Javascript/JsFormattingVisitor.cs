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
        List<(int index, CodeParameterInfo parameter)> parameters;
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
            var s = result.ToString();
            if (parameters != null) foreach (var p in parameters) {
                s = s.Insert(p.Item1, "~ [parameter: " + p.Item2.Parameter + "] ~");
            }
            return s;
        }

        public string GetParameterlessResult()
        {
            if (parameters != null) throw new InvalidOperationException($"The script contains parameters: `{ToString()}`.");
            return result.ToString();
        }

        public ParametrizedCode GetResult(OperatorPrecedence operatorPrecedence)
        {
            if (parameters == null || parameters.Count == 0) return new ParametrizedCode(new[] { result.ToString() }, null, operatorPrecedence);
            var parts = new string[parameters.Count + 1];
            parts[0] = result.ToString(0, parameters[0].index);
            for (int i = 1; i < parameters.Count; i++) {
                var from = parameters[i - 1].index;
                parts[i] = result.ToString(from, parameters[i].index - from);
            }
            int lastFrom = parameters[parameters.Count - 1].index;
            parts[parts.Length - 1] = result.ToString(lastFrom, result.Length - lastFrom);
            return new ParametrizedCode(parts, parameters.Select(p => p.parameter).ToArray(), operatorPrecedence);
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

        public void VisitAssignmentExpression(JsAssignmentExpression assignmentExpression)
        {
            assignmentExpression.Left.AcceptVisitor(this);
            EmitOperator(assignmentExpression.OperatorString);
            assignmentExpression.Right.AcceptVisitor(this);
        }

        public void VisitSymbolicParameter(JsSymbolicParameter symbolicParameter)
        {
            if (parameters == null) parameters = new List<(int, CodeParameterInfo)>();
            parameters.Add((result.Length, CodeParameterInfo.FromExpression(symbolicParameter)));
        }
    }
}