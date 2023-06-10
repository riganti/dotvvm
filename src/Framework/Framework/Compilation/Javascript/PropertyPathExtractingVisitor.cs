using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    internal class PropertyPathExtractingVisitor : JsNodeVisitor
    {
        private Stack<string> stack = new Stack<string>();
        private string wrongExpressionErrorMessage = "Provided path expression is invalid. Make sure it contains only property identifiers, member accesses and indexers with numeric literals.";

        public override void VisitArrowFunctionExpression(JsArrowFunctionExpression jsFunctionExpression) => throw new ArgumentException(wrongExpressionErrorMessage);
        public override void VisitAssignmentExpression(JsAssignmentExpression assignmentExpression) => throw new ArgumentException(wrongExpressionErrorMessage);
        public override void VisitBinaryExpression(JsBinaryExpression binaryExpression) => throw new ArgumentException(wrongExpressionErrorMessage);
        public override void VisitBlockStatement(JsBlockStatement jsBlockStatement) => throw new ArgumentException(wrongExpressionErrorMessage);
        public override void VisitConditionalExpression(JsConditionalExpression conditionalExpression) => throw new ArgumentException(wrongExpressionErrorMessage);
        public override void VisitFunctionExpression(JsFunctionExpression jsFunctionExpression) => throw new ArgumentException(wrongExpressionErrorMessage);
        public override void VisitIfStatement(JsIfStatement jsIfStatement) => throw new ArgumentException(wrongExpressionErrorMessage);
        public override void VisitInvocationExpression(JsInvocationExpression invocationExpression) => throw new ArgumentException(wrongExpressionErrorMessage);
        public override void VisitNewExpression(JsNewExpression newExpression) => throw new ArgumentException(wrongExpressionErrorMessage);
        public override void VisitParenthesizedExpression(JsParenthesizedExpression parenthesizedExpression) => throw new ArgumentException(wrongExpressionErrorMessage);
        public override void VisitReturnStatement(JsReturnStatement jsReturnStatement) => throw new ArgumentException(wrongExpressionErrorMessage);
        public override void VisitUnaryExpression(JsUnaryExpression unaryExpression) => throw new ArgumentException(wrongExpressionErrorMessage);
        public override void VisitVariableDefStatement(JsVariableDefStatement variableDefStatement) => throw new ArgumentException(wrongExpressionErrorMessage);

        public override void VisitMemberAccessExpression(JsMemberAccessExpression memberAccessExpression)
        {
            stack.Push(memberAccessExpression.MemberName);
            base.VisitMemberAccessExpression(memberAccessExpression);
        }

        public override void VisitIndexerExpression(JsIndexerExpression jsIndexerExpression)
        {
            stack.Push(jsIndexerExpression.Argument.ToString());
            base.VisitIndexerExpression(jsIndexerExpression);
        }

        public override void VisitIdentifierExpression(JsIdentifierExpression identifierExpression)
        {
            stack.Push(identifierExpression.Identifier.ToString());
            base.VisitIdentifierExpression(identifierExpression);
        }

        public string GetPropertyPath()
        {
            var sb = new StringBuilder();
            while (stack.Count > 0)
            {
                if (sb.Length > 0)
                    sb.Append($"/{stack.Pop()}");
                else
                    sb.Append(stack.Pop());
            }

            return sb.ToString();
        }
    }
}
