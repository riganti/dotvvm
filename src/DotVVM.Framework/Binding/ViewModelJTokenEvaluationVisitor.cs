using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Resources;

namespace DotVVM.Framework.Binding
{
    public class ViewModelJTokenEvaluationVisitor : CSharpSyntaxVisitor<JToken>
    {
        private Stack<JToken> hierarchy = new Stack<JToken>(); 

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelJTokenEvaluationVisitor"/> class.
        /// </summary>
        public ViewModelJTokenEvaluationVisitor(JToken root)
        {
            hierarchy.Push(root);
        }

        /// <summary>
        /// Backs up current state.
        /// </summary>
        public void BackUpCurrentPosition(JToken current)
        {
            hierarchy.Push(current);
        }

        /// <summary>
        /// Visits the expression statement.
        /// </summary>
        public override JToken VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            return Visit(node.Expression);
        }

        /// <summary>
        /// Visits the name of the identifier.
        /// </summary>
        public override JToken VisitIdentifierName(IdentifierNameSyntax node)
        {
            return GetObjectProperty(hierarchy.Peek(), node.Identifier.ToString());
        }

        /// <summary>
        /// Visits the member access expression.
        /// </summary>
        public override JToken VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            return GetObjectProperty(Visit(node.Expression), node.Name.ToString());
        }

        /// <summary>
        /// Visits the element access expression.
        /// </summary>
        public override JToken VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            var array = Visit(node.Expression) as JArray;
            if (array == null) return null;

            if (node.ArgumentList.Arguments.Count == 1)
            {
                var index = Visit(node.ArgumentList.Arguments.First()).Value<int?>();
                if (index != null)
                {
                    return array[index.Value];
                }
            }

            return base.VisitElementAccessExpression(node);
        }

        /// <summary>
        /// Visits the argument.
        /// </summary>
        public override JToken VisitArgument(ArgumentSyntax node)
        {
            return Visit(node.Expression);
        }

        /// <summary>
        /// Visits the literal expression.
        /// </summary>
        public override JToken VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                return node.Token.Value as int?;
            }

            return base.VisitLiteralExpression(node);
        }

        /// <summary>
        /// Gets the object property.
        /// </summary>
        private JToken GetObjectProperty(JToken target, string propertyName)
        {
            if (target == null) return null;

            if (propertyName == Constants.ParentSpecialBindingProperty)
            {
                hierarchy.Pop();
                return hierarchy.Peek();
            }
            else if (propertyName == Constants.RootSpecialBindingProperty)
            {
                while (hierarchy.Count > 1)
                {
                    hierarchy.Pop();
                }
                return hierarchy.Peek();
            }
            else if (propertyName == Constants.ThisSpecialBindingProperty)
            {
                return target;
            }
            else
            {
                return target[propertyName];
            }
        }

        public override JToken DefaultVisit(SyntaxNode node)
        {
            throw new ParserException(string.Format(Parser_Dothtml.Binding_UnsupportedExpressionInDataContext, node));
        }

    }
}