using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Redwood.Framework.Parser;
using Redwood.Framework.Resources;

namespace Redwood.Framework.Binding
{
    public class ExpressionEvaluationVisitor : CSharpSyntaxVisitor<object>
    {
        /// <summary>
        /// Gets or sets the context in which the expression is evaluated.
        /// </summary>
        public object DataContext { get; set; }

        /// <summary>
        /// Gets or sets the root context accessible by _root.
        /// </summary>
        public object Root { get; set; }

        /// <summary>
        /// Gets or sets a value whether the evaluator can return a MethodInfo.
        /// </summary>
        public bool AllowMethods { get; set; }

        /// <summary>
        /// The hierarchy of object hierarchy relative to the root at the time when expression evaluation finished.
        /// </summary>
        public Stack<object> Hierarchy { get; set; }

        /// <summary>
        /// Gets the target on which last property was evaluated.
        /// </summary>
        public object Target
        {
            get { return Hierarchy.Peek(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluationVisitor"/> class.
        /// </summary>
        public ExpressionEvaluationVisitor()
        {
            Hierarchy = new Stack<object>();
        }

        /// <summary>
        /// Visits the expression statement.
        /// </summary>
        public override object VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            return Visit(node.Expression);
        }

        /// <summary>
        /// Visits the name of the identifier.
        /// </summary>
        public override object VisitIdentifierName(IdentifierNameSyntax node)
        {
            return GetObjectProperty(DataContext, node.ToString());
        }

        /// <summary>
        /// Visits the element access expression.
        /// </summary>
        public override object VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            var array = Visit(node.Expression) as IList;
            if (array == null) return null;

            if (node.ArgumentList.Arguments.Count == 1)
            {
                var index = Visit(node.ArgumentList.Arguments.First()) as int?;
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
        public override object VisitArgument(ArgumentSyntax node)
        {
            return Visit(node.Expression);
        }

        /// <summary>
        /// Visits the literal expression.
        /// </summary>
        public override object VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                return node.Token.Value as int?;
            }
            return base.VisitLiteralExpression(node);
        }

        /// <summary>
        /// Visits the parenthesized expression.
        /// </summary>
        public override object VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            return Visit(node.Expression);
        }

        /// <summary>
        /// Visits the member access expression.
        /// </summary>
        public override object VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            return GetObjectProperty(Visit(node.Expression), node.Name.ToString());
        }

        /// <summary>
        /// Gets the specifiedproperty of a given object.
        /// </summary>
        private object GetObjectProperty(object target, string propertyName)
        {
            if (target == null) return null;

            if (propertyName == "_parent")
            {
                return Hierarchy.Pop();
            }
            else if (propertyName == "_root")
            {
                Hierarchy.Clear();
                return Root;
            }
            else if (propertyName == "_this")
            {
                return target;
            }
            else
            {
                Hierarchy.Push(target);
                var property = target.GetType().GetProperty(propertyName);
                if (property != null)
                {
                    // evaluate property
                    return property.GetValue(target);
                }
                else if (AllowMethods)
                {
                    // evaluate method
                    var methods = target.GetType().GetMethods().Where(n => n.Name == propertyName).ToList();
                    if (methods.Count == 0)
                    {
                        throw new ParserException(string.Format("The method {0} was not found on type {1}!", propertyName, target.GetType()));
                    }
                    else if (methods.Count > 1)
                    {
                        throw new ParserException(string.Format("The method {0} on type {1} is overloaded which is not supported yet!", propertyName, target.GetType()));
                    }
                    return methods[0];
                }
                else
                {
                    throw new ParserException(string.Format("The property {0} was not found on type {1}!", propertyName, target.GetType()));
                }
            }
        }

        public override object DefaultVisit(SyntaxNode node)
        {
            throw new ParserException(string.Format(Parser_RwHtml.Binding_UnsupportedExpression, node));
        }


    }
}