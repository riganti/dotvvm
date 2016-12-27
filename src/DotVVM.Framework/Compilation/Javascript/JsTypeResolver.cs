using DotVVM.Framework.Compilation.Javascript.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class JsNodeTypeAnnotation
    {
        public IJsTypeInfo Type { get; set; }
        public JsPropertyInfo Property { get; set; }
        public JsMethodSignature FunctionSignature { get; set; }
    }

    public class JsTypeResolver : IJsNodeVisitor
    {
        private void SetType(JsNode node, IJsTypeInfo type, JsPropertyInfo prop = null, JsMethodSignature sig = null)
        {
            var a = node.Annotation<JsNodeTypeAnnotation>() ?? node.AddAnnotation(new JsNodeTypeAnnotation());
            if (a.Type != null) a.Type = type;
            if (a.Property != null) a.Property = prop;
            if (a.FunctionSignature != null) a.FunctionSignature = sig;
        }
        public void VisitBinaryExpression(JsBinaryExpression binaryExpression)
        {
            VisitChildren(binaryExpression);
            switch (binaryExpression.Operator)
            {
                case BinaryOperatorType.Minus:
                case BinaryOperatorType.Times:
                case BinaryOperatorType.Divide:
                case BinaryOperatorType.Modulo:
                case BinaryOperatorType.Plus:
                case BinaryOperatorType.BitwiseAnd:
                case BinaryOperatorType.BitwiseOr:
                case BinaryOperatorType.BitwiseXOr:
                case BinaryOperatorType.LeftShift:
                case BinaryOperatorType.RightShift:
                case BinaryOperatorType.UnsignedRightShift:
                    SetType(binaryExpression, JsTypeInfo.Number);
                    break;
                case BinaryOperatorType.Equal:
                case BinaryOperatorType.NotEqual:
                case BinaryOperatorType.Greater:
                case BinaryOperatorType.GreaterOrEqual:
                case BinaryOperatorType.Less:
                case BinaryOperatorType.LessOrEqual:
                case BinaryOperatorType.StrictlyEqual:
                case BinaryOperatorType.StricltyNotEqual:
                case BinaryOperatorType.InstanceOf:
                case BinaryOperatorType.In:
                    SetType(binaryExpression, JsTypeInfo.Boolean);
                    break;
                case BinaryOperatorType.ConditionalAnd:
                case BinaryOperatorType.ConditionalOr:
                    SetType(binaryExpression, binaryExpression.Right.ResultType());
                    break;
                default: throw new NotSupportedException();
            }
        }

        public void VisitConditionalExpression(JsConditionalExpression conditionalExpression)
        {
            VisitChildren(conditionalExpression);
        }

        public void VisitIdentifier(JsIdentifier identifier) { }

        public void VisitIdentifierExpression(JsIdentifierExpression identifierExpression)
        {
            SetType(identifierExpression, JsAnyType.Instance);
        }

        public void VisitIndexerExpression(JsIndexerExpression indexerExpression)
        {
            VisitChildren(indexerExpression);
            var type = indexerExpression.Target.ResultType();
            if (type is IJsIndexerAccess indexerType && (!indexerType.IntegerOnly || JsTypeInfo.Number.MatchType(indexerExpression.Argument.ResultType())))
            {
                SetType(indexerExpression, indexerType.ElementType);
            }
            else
            {
                SetType(indexerExpression, JsTypeInfo.Error);
            }
        }

        public void VisitInvocationExpression(JsInvocationExpression invocationExpression)
        {
            VisitChildren(invocationExpression);
            var target = invocationExpression.ResultType();
            var args = invocationExpression.Arguments.Select(JsTypeHelpers.ResultType).ToArray();
            if (target is IJsInvocableType invocable &&
                invocable.GetSignatures().FirstOrDefault(s => s.MatchArgs(args)) is var sig &&
                sig != null)
                SetType(invocationExpression, sig.ResultType, sig: sig);
            else SetType(invocationExpression, JsTypeInfo.Error);
        }

        public void VisitLiteral(JsLiteral jsLiteral)
        {
            throw new NotImplementedException();
        }

        public void VisitMemberAccessExpression(JsMemberAccessExpression memberAccessExpression)
        {
            VisitChildren(memberAccessExpression);
            var type = memberAccessExpression.Target.ResultType();
            if (type is IJsMemberAccessType mtype &&
                mtype.GetProperty(memberAccessExpression.MemberName) is JsPropertyInfo property)
                SetType(memberAccessExpression, property.Type, prop: property);
        }

        public void VisitParenthesizedExpression(JsParenthesizedExpression parenthesizedExpression)
        {
            VisitChildren(parenthesizedExpression);
            SetType(parenthesizedExpression, parenthesizedExpression.Expression.ResultType());
        }

        public void VisitUnaryExpression(JsUnaryExpression unaryExpression)
        {
            VisitChildren(unaryExpression);
            switch (unaryExpression.Operator)
            {
                case UnaryOperatorType.LogicalNot:
                case UnaryOperatorType.Delete:
                    SetType(unaryExpression, JsTypeInfo.Boolean);
                    break;
                case UnaryOperatorType.Void:
                    SetType(unaryExpression, NullOrUndefinedJsType.Undefined);
                    break;
                case UnaryOperatorType.TypeOf:
                    SetType(unaryExpression, JsTypeInfo.String);
                    break;
                case UnaryOperatorType.Plus:
                case UnaryOperatorType.BitwiseNot:
                case UnaryOperatorType.Minus:
                case UnaryOperatorType.Increment:
                case UnaryOperatorType.Decrement:
                    SetType(unaryExpression, JsTypeInfo.Number);
                    break;
                default:
                    break;
            }
        }

        protected void VisitChildren(JsNode node)
        {
            foreach (var c in node.Children) c.AcceptVisitor(this);
        }

        public void VisitAssignmentExpression(JsAssignmentExpression assignmentExpression)
        {
            SetType(assignmentExpression, assignmentExpression.Right.ResultType());
        }

        public void VisitSymbolicParameter(JsSymbolicParameter symbolicParameter) { }
    }

    public static class JsTypeHelpers
    {
        public static IJsTypeInfo ResultType(this JsNode node) => node.Annotation<JsNodeTypeAnnotation>()?.Type;
        public static TNode AddTypeInfo<TNode>(this TNode node, IJsTypeInfo type, JsPropertyInfo prop, JsMethodSignature sig)
            where TNode : JsNode
        {
            var a = node.Annotation<JsNodeTypeAnnotation>() ?? node.AddAnnotation(new JsNodeTypeAnnotation());
            if (type != null) a.Type = type;
            if (prop != null) a.Property = prop;
            if (sig != null) a.FunctionSignature = sig;
            return node;
        }
    }
}
