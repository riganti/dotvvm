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
    }
}
