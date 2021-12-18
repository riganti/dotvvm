using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Utils;

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
        List<(int index, CodeParameterInfo parameter)>? parameters;


        protected int? lastParameterIndex => parameters is null or { Count: 0 } ? null : parameters[parameters.Count - 1].index;
        protected bool endsWithParameter => lastParameterIndex == result.Length;

        protected void Emit(string str)
        {
            Debug.Assert(!str.Contains("\n"));
            result.Append(str);
        }
        protected void Emit(char ch)
        {
            Debug.Assert(ch != '\n');
            result.Append(ch);
        }

        protected void CommitLine()
        {
            if (NiceMode)
            {
                while (result.Length > 0 && result[result.Length - 1] == ' ' && !endsWithParameter) result.Remove(result.Length - 1, 1);

                result.Append('\n');
                for (int i = 0; i < indentLevel; i++)
                {
                    result.Append(IndentString);
                }
            }
        }

        protected void OptionalSpace()
        {
            if (!NiceMode) return;
            var endsWithCharacter =
                result.Length > 0 && !char.IsWhiteSpace(result[result.Length - 1]);
            if (endsWithCharacter || endsWithParameter) Emit(' ');
        }

        static bool IsOperatorChar(char ch) => ch == '+' | ch == '-' | ch == '&' | ch == '|' | ch == '?' | ch == '=' | ch == '*' | ch == '/';
        static bool IsIdentifierChar(char ch) => char.IsLetterOrDigit(ch) | ch == '_' | ch == '$';
        static bool IsDangerousTuple(char a, char b) => IsOperatorChar(a) && (a == b | b == '=') || IsIdentifierChar(a) && IsIdentifierChar(b);

        public static bool NeedSpaceBetween(StringBuilder a, string b)
        {
            if (a.Length == 0 || b.Length == 0)
                return false;
            if (char.IsWhiteSpace(a[a.Length - 1]) || char.IsWhiteSpace(b[0]))
                return false;
            return IsDangerousTuple(a[a.Length - 1], b[0]);
        }
        protected void SpaceBeforeOp(string op, bool allowCosmeticSpace = true)
        {
            var needsSpace =
                NeedSpaceBetween(result, op) &&
                !endsWithParameter;

            if (needsSpace)
                Emit(' ');
            else if (allowCosmeticSpace) OptionalSpace();
        }

        protected void EmitOperator(string op, bool allowCosmeticSpace = true)
        {
            SpaceBeforeOp(op, allowCosmeticSpace);
            Emit(op);
            if (allowCosmeticSpace) OptionalSpace();
        }

        protected void EndStatement()
        {
            Emit(';');
            CommitLine();
        }

        int indentLevel = 0;
        protected void Indent()
        {
            indentLevel++;
        }
        protected void Dedent()
        {
            // remove last space
            var expectedIndentLength = IndentString.Length * indentLevel;
            if (NiceMode && result.Length > expectedIndentLength)
            {
                var indent = result.ToString(result.Length - expectedIndentLength - 1, expectedIndentLength + 1);
                var removeOne = indent[0] == '\n';
                for (var i = 0; i < expectedIndentLength; i++)
                {
                    removeOne |= indent[i + 1] == IndentString[i % IndentString.Length];
                }
                if (removeOne)
                {
                    result.Remove(result.Length - IndentString.Length, IndentString.Length);
                }
            }

            indentLevel--;
        }

        public override string ToString()
        {
            var s = result.ToString();
            if (parameters != null) foreach (var p in Enumerable.Reverse(parameters))
                {
                    s = s.Insert(p.index, "$" + Math.Abs(p.parameter.Parameter.GetHashCode()));
                }
            return s;
        }

        public string GetParameterlessResult()
        {
            return GetResult(OperatorPrecedence.Max).ToDefaultString();
        }

        public ParametrizedCode GetResult(OperatorPrecedence operatorPrecedence)
        {
            if (parameters == null || parameters.Count == 0) return new ParametrizedCode(result.ToString(), operatorPrecedence);
            var parts = new string[parameters.Count + 1];
            parts[0] = result.ToString(0, parameters[0].index).DotvvmInternString();
            for (int i = 1; i < parameters.Count; i++)
            {
                var from = parameters[i - 1].index;
                parts[i] = result.ToString(from, parameters[i].index - from).DotvvmInternString();
            }
            int lastFrom = parameters[parameters.Count - 1].index;
            parts[parts.Length - 1] = result.ToString(lastFrom, result.Length - lastFrom).DotvvmInternString();
            return new ParametrizedCode(parts, parameters.Select(p => p.parameter).ToArray(), operatorPrecedence);
        }

        protected void VisitChildren(JsNode node)
        {
            foreach (var c in node.Children)
            {
                c.AcceptVisitor(this);
            }
        }

        public void VisitBinaryExpression(JsBinaryExpression binaryExpression)
        {
            binaryExpression.Left.AcceptVisitor(this);
            var op = binaryExpression.OperatorString;
            EmitOperator(op);
            if (binaryExpression.Operator == BinaryOperatorType.Sequence)
            {
                CommitLine();
            }
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
            EmitOperator(identifier.Name, allowCosmeticSpace: false);
        }

        public void VisitMemberAccessExpression(JsMemberAccessExpression memberAccessExpression)
        {
            if (!memberAccessExpression.MemberNameToken.IsValidName())
                new JsIndexerExpression(memberAccessExpression.Target.Clone(), new JsLiteral(memberAccessExpression.MemberNameToken))
                .AcceptVisitor(this);
            else
            {
                memberAccessExpression.Target.AcceptVisitor(this);
                Emit(memberAccessExpression.IsOptional ? "?." : ".");
                memberAccessExpression.MemberNameToken.AcceptVisitor(this);
            }
        }

        public void VisitIdentifierExpression(JsIdentifierExpression identifierExpression)
        {
            identifierExpression.IdentifierToken.AcceptVisitor(this);
        }

        public void VisitInvocationExpression(JsInvocationExpression invocationExpression)
        {
            invocationExpression.Target.AcceptVisitor(this);
            Emit('(');
            int i = 0;
            foreach (var arg in invocationExpression.Arguments)
            {
                if (i++ > 0) { Emit(','); OptionalSpace(); }
                arg.AcceptVisitor(this);
            }
            Emit(')');
        }

        public void VisitParenthesizedExpression(JsParenthesizedExpression parenthesizedExpression)
        {
            bool isSequenceBlock = parenthesizedExpression.Expression is JsBinaryExpression binaryExpression && binaryExpression.Operator == BinaryOperatorType.Sequence;
            Emit('(');
            if (isSequenceBlock)
            {
                Indent();
                CommitLine();
            }
            parenthesizedExpression.Expression.AcceptVisitor(this);
            if (isSequenceBlock)
            {
                CommitLine();
                Dedent();
            }
            Emit(')');
        }

        public void VisitUnaryExpression(JsUnaryExpression unaryExpression)
        {
            if (unaryExpression.IsPrefix)
            {
                SpaceBeforeOp(unaryExpression.OperatorString, allowCosmeticSpace: false);
                Emit(unaryExpression.OperatorString);
                unaryExpression.Expression.AcceptVisitor(this);
            }
            else
            {
                unaryExpression.Expression.AcceptVisitor(this);
                SpaceBeforeOp(unaryExpression.OperatorString, allowCosmeticSpace: false);
                Emit(unaryExpression.OperatorString);
            }
        }

        public void VisitIndexerExpression(JsIndexerExpression indexerExpression)
        {
            indexerExpression.Target.AcceptVisitor(this);
            Emit('[');
            indexerExpression.Argument.AcceptVisitor(this);
            Emit(']');
        }

        public void VisitLiteral(JsLiteral jsLiteral)
        {
            var literalValue = jsLiteral.LiteralValue;
            if (char.IsLetterOrDigit(literalValue[0])) SpaceBeforeOp(literalValue, allowCosmeticSpace: false);
            Emit(literalValue);
        }

        public void VisitAssignmentExpression(JsAssignmentExpression assignmentExpression)
        {
            assignmentExpression.Left.AcceptVisitor(this);
            EmitOperator(assignmentExpression.OperatorString);
            assignmentExpression.Right.AcceptVisitor(this);
        }

        public void VisitSymbolicParameter(JsSymbolicParameter symbolicParameter)
        {
            SpaceBeforeOp("X", allowCosmeticSpace: false);
            if (parameters == null) parameters = new List<(int, CodeParameterInfo)>();
            parameters.Add((result.Length, CodeParameterInfo.FromExpression(symbolicParameter)));
        }

        public void VisitObjectExpression(JsObjectExpression objectExpression)
        {
            if (objectExpression.Parent is JsExpressionStatement) Emit('(');
            Emit('{');
            Indent();
            var first = true;
            foreach (var item in objectExpression.Properties)
            {
                if (!first) { Emit(','); OptionalSpace(); }
                else first = false;

                if (objectExpression.Properties.Count > 1) CommitLine();

                item.AcceptVisitor(this);
            }
            Dedent();
            if (objectExpression.Properties.Count > 1) CommitLine();
            Emit('}');
            if (objectExpression.Parent is JsExpressionStatement) Emit(')');
        }

        public void VisitExpressionStatement(JsExpressionStatement jsExpressionStatement)
        {
            jsExpressionStatement.Expression.AcceptVisitor(this);
            EndStatement();
        }

        public void VisitReturnStatement(JsReturnStatement jsReturnStatement)
        {
            Emit("return ");
            jsReturnStatement.Expression.AcceptVisitor(this);
            EndStatement();
        }

        public void VisitArrayExpression(JsArrayExpression jsArrayExpression)
        {
            Emit('[');
            Indent();
            var first = true;
            foreach (var item in jsArrayExpression.Arguments)
            {
                if (jsArrayExpression.Arguments.Count > 1) CommitLine();
                if (!first) { Emit(','); OptionalSpace(); }
                else first = false;

                item.AcceptVisitor(this);
            }
            Dedent();
            if (jsArrayExpression.Arguments.Count > 1) CommitLine();
            Emit(']');
        }

        public void VisitBlockStatement(JsBlockStatement blockStatement)
        {
            Emit('{');
            Indent();
            CommitLine();
            foreach (var ss in blockStatement.Body)
            {
                ss.AcceptVisitor(this);
            }
            Dedent();
            Emit('}');

        }

        public void VisitVariableDefStatement(JsVariableDefStatement variableDefStatement)
        {
            Emit(variableDefStatement.Keyword);
            Emit(' ');
            variableDefStatement.NameIdentifier.AcceptVisitor(this);
            if (variableDefStatement.Initialization is object)
            {
                EmitOperator("=");
                variableDefStatement.Initialization.AcceptVisitor(this);
            }
            EndStatement();
        }

        public void VisitIfStatement(JsIfStatement ifStatement)
        {
            Emit("if(");
            ifStatement.Condition.AcceptVisitor(this);
            Emit(')');
            OptionalSpace();
            ifStatement.TrueBranch.AcceptVisitor(this);
            CommitLine();
            if (ifStatement.FalseBranch != null)
            {
                Emit("else ");
                ifStatement.FalseBranch.AcceptVisitor(this);
                CommitLine();
            }
        }
        public void VisitArrowFunctionExpression(JsArrowFunctionExpression functionExpression)
        {
            if (functionExpression.IsAsync)
            {
                Emit("async ");
            }
            EmitOperator("(", allowCosmeticSpace: false);
            var first = true;
            foreach (var item in functionExpression.Parameters)
            {
                if (!first) { Emit(','); OptionalSpace(); }
                else first = false;

                item.AcceptVisitor(this);
            }
            Emit(')');
            OptionalSpace();
            Emit("=>");
            OptionalSpace();

            // `() => { return X }` is written out as `() => X`
            if (functionExpression.ExpressionBody is {} exprBody)
            {
                var needsParens = exprBody is JsBinaryExpression { Operator: BinaryOperatorType.Sequence } ||
                                  exprBody is JsObjectExpression ||
                                  exprBody is JsLiteral literal && literal.LiteralValue.StartsWith("{");
                if (needsParens)
                    Emit('(');
                
                exprBody.AcceptVisitor(this);

                if (needsParens)
                    Emit(')');
            }
            else
            {
                functionExpression.Block.AcceptVisitor(this);
            }
        }

        public void VisitFunctionExpression(JsFunctionExpression functionExpression)
        {
            if (functionExpression.IsAsync)
            {
                Emit("async ");
            }
            EmitOperator("function(", allowCosmeticSpace: false);
            var first = true;
            foreach (var item in functionExpression.Parameters)
            {
                if (!first) { Emit(','); OptionalSpace(); }
                else first = false;

                item.AcceptVisitor(this);
            }
            Emit(')');
            OptionalSpace();
            functionExpression.Block.AcceptVisitor(this);
        }

        public void VisitObjectProperty(JsObjectProperty objectProperty)
        {
            if (objectProperty.Identifier.IsValidName())
                objectProperty.Identifier.AcceptVisitor(this);
            else new JsLiteral(objectProperty.Identifier.Name).AcceptVisitor(this);
            Emit(':');
            OptionalSpace();
            objectProperty.Expression.AcceptVisitor(this);
        }

        public void VisitNewExpression(JsNewExpression newExpression)
        {
            EmitOperator("new ", allowCosmeticSpace: false);
            newExpression.Target.AcceptVisitor(this);
            Emit('(');
            int i = 0;
            foreach (var arg in newExpression.Arguments)
            {
                if (i++ > 0) { Emit(','); OptionalSpace(); }
                arg.AcceptVisitor(this);
            }
            Emit(')');
        }
    }
}
