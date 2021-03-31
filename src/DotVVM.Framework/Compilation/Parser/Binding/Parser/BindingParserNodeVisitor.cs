#nullable enable
using System;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    public abstract class BindingParserNodeVisitor<T>
    {

        public virtual T Visit(BindingParserNode node)
        {
            if (node is ArrayAccessBindingParserNode)
            {
                return VisitArrayAccess((ArrayAccessBindingParserNode)node);
            }
            else if (node is BinaryOperatorBindingParserNode)
            {
                return VisitBinaryOperator((BinaryOperatorBindingParserNode)node);
            }
            else if (node is ConditionalExpressionBindingParserNode)
            {
                return VisitConditionalExpression((ConditionalExpressionBindingParserNode)node);
            }
            else if (node is FunctionCallBindingParserNode)
            {
                return VisitFunctionCall((FunctionCallBindingParserNode)node);
            }
            else if (node is GenericNameBindingParserNode)
            {
                return VisitGenericName((GenericNameBindingParserNode)node);
            }
            else if (node is SimpleNameBindingParserNode)
            {
                return VisitSimpleName((SimpleNameBindingParserNode)node);
            }
            else if (node is LiteralExpressionBindingParserNode)
            {
                return VisitLiteralExpression((LiteralExpressionBindingParserNode)node);
            }
            else if (node is InterpolatedStringBindingParserNode)
            {
                return VisitInterpolatedStringExpression((InterpolatedStringBindingParserNode)node);
            }
            else if (node is MemberAccessBindingParserNode)
            {
                return VisitMemberAccess((MemberAccessBindingParserNode)node);
            }
            else if (node is ParenthesizedExpressionBindingParserNode)
            {
                return VisitParenthesizedExpression((ParenthesizedExpressionBindingParserNode)node);
            }
            else if (node is UnaryOperatorBindingParserNode)
            {
                return VisitUnaryOperator((UnaryOperatorBindingParserNode)node);
            }
            else if (node is LambdaBindingParserNode)
            {
                return VisitLambda((LambdaBindingParserNode)node);
            }
            else if (node is LambdaParameterBindingParserNode)
            {
                return VisitLambdaParameter((LambdaParameterBindingParserNode)node);
            }
            else if (node is MultiExpressionBindingParserNode)
            {
                return VisitMultiExpression((MultiExpressionBindingParserNode)node);
            }
            else if (node is AssemblyQualifiedNameBindingParserNode)
            {
                return VisitAssemblyQualifiedName((AssemblyQualifiedNameBindingParserNode)node);
            }
            else if (node is BlockBindingParserNode blockNode)
            {
                return VisitBlock(blockNode);
            }
            else if (node is VoidBindingParserNode voidNode)
            {
                return VisitVoid(voidNode);
            }
            else if (node is null)
            {
                return default(T)!;
            }
            else
            {
                throw new NotSupportedException($"The node of type {node.GetType()} is not supported!");
            }
        }

        protected virtual T VisitLambda(LambdaBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitLambdaParameter(LambdaParameterBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitGenericName(GenericNameBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitArrayAccess(ArrayAccessBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitBinaryOperator(BinaryOperatorBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitConditionalExpression(ConditionalExpressionBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitFunctionCall(FunctionCallBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitSimpleName(SimpleNameBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitLiteralExpression(LiteralExpressionBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitInterpolatedStringExpression(InterpolatedStringBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitMemberAccess(MemberAccessBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitParenthesizedExpression(ParenthesizedExpressionBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitUnaryOperator(UnaryOperatorBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitMultiExpression(MultiExpressionBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitAssemblyQualifiedName(AssemblyQualifiedNameBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitBlock(BlockBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T VisitVoid(VoidBindingParserNode node)
        {
            return DefaultVisit(node);
        }

        protected virtual T DefaultVisit(BindingParserNode node)
        {
            throw new NotImplementedException("The visitor implementation should implement this method!");
        }
    }
}
