using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public interface IJsNodeVisitor
    {
        void VisitBinaryExpression(JsBinaryExpression binaryExpression);
        void VisitIdentifier(JsIdentifier identifier);
        void VisitMemberAccessExpression(JsMemberAccessExpression memberAccessExpression);
        void VisitIdentifierExpression(JsIdentifierExpression identifierExpression);
        void VisitSymbolicParameter(JsSymbolicParameter symbolicParameter);
        void VisitObjectExpression(JsObjectExpression jsObjectExpression);
        void VisitExpressionStatement(JsExpressionStatement jsExpressionStatement);
        void VisitReturnStatement(JsReturnStatement jsReturnStatement);
        void VisitArrayExpression(JsArrayExpression jsArrayExpression);
        void VisitInvocationExpression(JsInvocationExpression invocationExpression);
        void VisitBlockStatement(JsBlockStatement jsBlockStatement);
        void VisitParenthesizedExpression(JsParenthesizedExpression parenthesizedExpression);
        void VisitUnaryExpression(JsUnaryExpression unaryExpression);
        void VisitLiteral(JsLiteral jsLiteral);
        void VisitIndexerExpression(JsIndexerExpression indexerExpression);
        void VisitNewExpression(JsNewExpression newExpression);
        void VisitConditionalExpression(JsConditionalExpression conditionalExpression);
        void VisitAssignmentExpression(JsAssignmentExpression assignmentExpression);
        void VisitIfStatement(JsIfStatement jsIfStatement);
        void VisitFunctionExpression(JsFunctionExpression jsFunctionExpression);
        void VisitObjectProperty(JsObjectProperty jsObjectProperty);
    }

    public class JsNodeVisitor : IJsNodeVisitor
    {
        public virtual void VisitArrayExpression(JsArrayExpression jsArrayExpression) => DefaultVisit(jsArrayExpression);

        public virtual void VisitAssignmentExpression(JsAssignmentExpression assignmentExpression) => DefaultVisit(assignmentExpression);

        public virtual void VisitBinaryExpression(JsBinaryExpression binaryExpression) => DefaultVisit(binaryExpression);

        public virtual void VisitBlockStatement(JsBlockStatement jsBlockStatement) => DefaultVisit(jsBlockStatement);

        public virtual void VisitConditionalExpression(JsConditionalExpression conditionalExpression) => DefaultVisit(conditionalExpression);

        public virtual void VisitExpressionStatement(JsExpressionStatement jsExpressionStatement) => DefaultVisit(jsExpressionStatement);

        public virtual void VisitFunctionExpression(JsFunctionExpression jsFunctionExpression) => DefaultVisit(jsFunctionExpression);

        public virtual void VisitIdentifier(JsIdentifier identifier) => DefaultVisit(identifier);

        public virtual void VisitIdentifierExpression(JsIdentifierExpression identifierExpression) => DefaultVisit(identifierExpression);

        public virtual void VisitIfStatement(JsIfStatement jsIfStatement) => DefaultVisit(jsIfStatement);

        public virtual void VisitIndexerExpression(JsIndexerExpression indexerExpression) => DefaultVisit(indexerExpression);

        public virtual void VisitInvocationExpression(JsInvocationExpression invocationExpression) => DefaultVisit(invocationExpression);

        public virtual void VisitLiteral(JsLiteral jsLiteral) => DefaultVisit(jsLiteral);

        public virtual void VisitMemberAccessExpression(JsMemberAccessExpression memberAccessExpression) => DefaultVisit(memberAccessExpression);

        public virtual void VisitNewExpression(JsNewExpression newExpression) => DefaultVisit(newExpression);

        public virtual void VisitObjectExpression(JsObjectExpression jsObjectExpression) => DefaultVisit(jsObjectExpression);

        public virtual void VisitObjectProperty(JsObjectProperty jsObjectProperty) => DefaultVisit(jsObjectProperty);

        public virtual void VisitParenthesizedExpression(JsParenthesizedExpression parenthesizedExpression) => DefaultVisit(parenthesizedExpression);

        public virtual void VisitReturnStatement(JsReturnStatement jsReturnStatement) => DefaultVisit(jsReturnStatement);

        public virtual void VisitSymbolicParameter(JsSymbolicParameter symbolicParameter) => DefaultVisit(symbolicParameter);

        public virtual void VisitUnaryExpression(JsUnaryExpression unaryExpression) => DefaultVisit(unaryExpression);

        protected virtual void DefaultVisit(JsNode node)
        {
            foreach (var c in node.Children)
                c.AcceptVisitor(this);
        }
    }
}
