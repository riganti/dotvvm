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
        void VisitInvocationExpression(JsInvocationExpression invocationExpression);
        void VisitParenthesizedExpression(JsParenthesizedExpression parenthesizedExpression);
        void VisitUnaryExpression(JsUnaryExpression unaryExpression);
        void VisitLiteral(JsLiteral jsLiteral);
        void VisitIndexerExpression(JsIndexerExpression indexerExpression);
        void VisitConditionalExpression(JsConditionalExpression conditionalExpression);
        void VisitAssignmentExpression(JsAssignmentExpression assignmentExpression);
    }

    public class JsNodeVisitor : IJsNodeVisitor
    {
        public virtual void VisitAssignmentExpression(JsAssignmentExpression assignmentExpression) => DefaultVisit(assignmentExpression);

        public virtual void VisitBinaryExpression(JsBinaryExpression binaryExpression) => DefaultVisit(binaryExpression);

        public virtual void VisitConditionalExpression(JsConditionalExpression conditionalExpression) => DefaultVisit(conditionalExpression);

        public virtual void VisitIdentifier(JsIdentifier identifier) => DefaultVisit(identifier);

        public virtual void VisitIdentifierExpression(JsIdentifierExpression identifierExpression) => DefaultVisit(identifierExpression);

        public virtual void VisitIndexerExpression(JsIndexerExpression indexerExpression) => DefaultVisit(indexerExpression);

        public virtual void VisitInvocationExpression(JsInvocationExpression invocationExpression) => DefaultVisit(invocationExpression);

        public virtual void VisitLiteral(JsLiteral jsLiteral) => DefaultVisit(jsLiteral);

        public virtual void VisitMemberAccessExpression(JsMemberAccessExpression memberAccessExpression) => DefaultVisit(memberAccessExpression);

        public virtual void VisitParenthesizedExpression(JsParenthesizedExpression parenthesizedExpression) => DefaultVisit(parenthesizedExpression);

        public virtual void VisitUnaryExpression(JsUnaryExpression unaryExpression) => DefaultVisit(unaryExpression);

        protected virtual void DefaultVisit(JsNode node)
        {
            foreach (var c in node.Children)
            {
                c.AcceptVisitor(this);
            }
        }
    }
}
