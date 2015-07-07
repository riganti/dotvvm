using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DotVVM.Framework.Resources;

namespace DotVVM.Framework.Parser.Translation
{
    public class ExpressionTranslatorVisitor : CSharpSyntaxVisitor<string>
    {
        public bool UseNullPropagation { get; set; }

        /// <summary>
        /// Gets a value indicating whether the syntax contains an expression that prevents us to pass the knockout observable as a result.
        /// </summary>
        public bool IsExpression { get; private set; }


        public ExpressionTranslatorVisitor()
        {
            UseNullPropagation = true;
        }

        /// <summary>
        /// Visits the prefix unary expression.
        /// </summary>
        public override string VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            IsExpression = true;

            if (node.OperatorToken.IsKind(SyntaxKind.MinusToken))
            {
                return "-" + Visit(node.Operand);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.ExclamationToken))
            {
                return "!" + Visit(node.Operand);
            }

            throw new ParserException(string.Format(Parser_Dothtml.Binding_UnsupportedOperator, node.OperatorToken.Text));
        }

        /// <summary>
        /// Visits the binary expression.
        /// </summary>
        public override string VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            IsExpression = true;

            // arithmetic
            if (node.OperatorToken.IsKind(SyntaxKind.PlusToken))
            {
                return Visit(node.Left) + " + " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.MinusToken))
            {
                return Visit(node.Left) + " - " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.AsteriskToken))
            {
                return Visit(node.Left) + " * " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.SlashToken))
            {
                return Visit(node.Left) + " / " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.PercentToken))
            {
                return Visit(node.Left) + " % " + Visit(node.Right);
            }

            // comparison
            if (node.OperatorToken.IsKind(SyntaxKind.LessThanToken))
            {
                return Visit(node.Left) + " < " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.LessThanEqualsToken))
            {
                return Visit(node.Left) + " <= " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.GreaterThanToken))
            {
                return Visit(node.Left) + " > " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.GreaterThanEqualsToken))
            {
                return Visit(node.Left) + " >= " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken))
            {
                return Visit(node.Left) + " === " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken))
            {
                return Visit(node.Left) + " !== " + Visit(node.Right);
            }

            // null coalescing operator
            if (node.OperatorToken.IsKind(SyntaxKind.QuestionQuestionToken))
            {
                return Visit(node.Left) + " !== null ? " + Visit(node.Left) + " : " + Visit(node.Right);
            }

            throw new ParserException(string.Format(Parser_Dothtml.Binding_UnsupportedOperator, node.OperatorToken.Text));
        }

        /// <summary>
        /// Visits the conditional expression.
        /// </summary>
        public override string VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            IsExpression = true;

            return Visit(node.Condition) + " ? " + Visit(node.WhenTrue) + " : " + Visit(node.WhenFalse);
        }

        /// <summary>
        /// Visits the name of the identifier.
        /// </summary>
        public override string VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Identifier.Text == Constants.ParentSpecialBindingProperty)
            {
                return "$parent";
            }
            else if (node.Identifier.Text == Constants.RootSpecialBindingProperty)
            {
                return "$root";
            }
            else if (node.Identifier.Text == Constants.ThisSpecialBindingProperty)
            {
                return "$data";
            }
            else
            {
                return node.Identifier.Text + "()";
            }
        }

        /// <summary>
        /// Visits the parenthesized expression.
        /// </summary>
        public override string VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            IsExpression = true;

            return "(" + Visit(node.Expression) + ")";
        }

        /// <summary>
        /// Visits the member access expression.
        /// </summary>
        public override string VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (UseNullPropagation)
            {
                return "(" + Visit(node.Expression) + "||{})." + Visit(node.Name);
            }
            else
            {
                return Visit(node.Expression) + "." + Visit(node.Name);
            }
        }

        /// <summary>
        /// Visits the literal expression.
        /// </summary>
        public override string VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return node.ToString();
            }
            else if (node.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                return Convert.ToDouble(node.Token.Value).ToString(CultureInfo.InvariantCulture);
            }
            else if (node.IsKind(SyntaxKind.NullLiteralExpression))
            {
                return "null";
            }
            else if (node.IsKind(SyntaxKind.TrueLiteralExpression))
            {
                return "true";
            }
            else if (node.IsKind(SyntaxKind.FalseLiteralExpression))
            {
                return "false";
            }
            
            return base.VisitLiteralExpression(node);
        }

        /// <summary>
        /// Visits the element access expression.
        /// </summary>
        public override string VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            if (node.ArgumentList.Arguments.Count == 1)
            {
                return Visit(node.Expression) + "[" + Visit(node.ArgumentList.Arguments.First()) + "]";
            }

            return base.VisitElementAccessExpression(node);
        }

        /// <summary>
        /// Visits the argument.
        /// </summary>
        public override string VisitArgument(ArgumentSyntax node)
        {
            return Visit(node.Expression);
        }

        public override string DefaultVisit(SyntaxNode node)
        {
            throw new ParserException(string.Format(Parser_Dothtml.Binding_UnsupportedExpression, node));
        }
    }
}