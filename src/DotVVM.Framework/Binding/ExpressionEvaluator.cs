using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Parser;

namespace DotVVM.Framework.Binding
{
    public class ExpressionEvaluator
    {

        public bool AllowMethods { get; set; }


        
        /// <summary>
        /// Evaluates the specified expression in the context of specified control.
        /// </summary>
        public object Evaluate(ValueBindingExpression expression, DotvvmProperty property, DotvvmBindableControl contextControl)
        {
            var visitor = EvaluateDataContextPath(contextControl);

            // evaluate the final expression
            EvaluateBinding(visitor, expression.Expression, expression.GetViewModelPathExpression(contextControl, property));
            return visitor.Result;
        }

        /// <summary>
        /// Evaluates the data context path and returns the visitor with hierarchy.
        /// </summary>
        private ExpressionEvaluationVisitor EvaluateDataContextPath(DotvvmBindableControl contextControl)
        {
            // get the hierarchy of DataContext the control is in
            var dataContexts = contextControl.GetAllAncestors().OfType<DotvvmBindableControl>()
                .Select(c => new { Binding = c.GetBinding(DotvvmBindableControl.DataContextProperty, false), Control = c })
                .Where(b => b.Binding != null)
                .ToList();

            // evaluate the DataContext path
            var viewRoot = contextControl.GetRoot();
            var visitor = new ExpressionEvaluationVisitor(GetRootDataContext(viewRoot), viewRoot) { AllowMethods = AllowMethods };
            for (var i = dataContexts.Count - 1; i >= 0; i--)
            {
                var binding = dataContexts[i].Binding;
                if (!(binding is ValueBindingExpression))
                {
                    throw new Exception("The DataContext property can only contain value bindings!"); // TODO: exception handling
                }
                var pathExpression = ((ValueBindingExpression)binding).GetViewModelPathExpression(dataContexts[i].Control, DotvvmBindableControl.DataContextProperty);
                EvaluateBinding(visitor, binding.Expression, pathExpression);
            }
            return visitor;
        }

        /// <summary>
        /// Evaluates the binding.
        /// </summary>
        internal object EvaluateBinding(ExpressionEvaluationVisitor visitor, string expression, string pathExpression)
        {
            // parse
            var node = ParseBinding(expression);
            var result = visitor.Visit(node);
            visitor.BackupCurrentPosition(result, pathExpression);

            return result;
        }

        /// <summary>
        /// Parses the binding.
        /// </summary>
        private SyntaxNode ParseBinding(string expression)
        {
            var tree = CSharpSyntaxTree.ParseText(expression, new CSharpParseOptions(LanguageVersion.CSharp5, DocumentationMode.Parse, SourceCodeKind.Interactive));

            // make sure it is a single statement expression
            var statement = tree.GetRoot().ChildNodes().First() as GlobalStatementSyntax;
            if (statement == null)
            {
                throw new ParserException("The expression in binding must be a compilable C# expression!");
            }
            var expr = statement.ChildNodes().OfType<ExpressionStatementSyntax>().FirstOrDefault();
            if (expr == null)
            {
                throw new ParserException("The expression in binding must be a compilable C# expression!");
            }
            var node = expr.ChildNodes().First();
            return node;
        }


        /// <summary>
        /// Evaluates the PropertyInfo for the expression.
        /// </summary>
        public PropertyInfo EvaluateProperty(ValueBindingExpression expression, DotvvmProperty property, DotvvmBindableControl control, out object target)
        {
            var visitor = EvaluateDataContextPath(control);
            target = visitor.Result;

            // evaluate the final expression
            var node = ParseBinding(expression.Expression);
            string propertyName = null;
            if (node is IdentifierNameSyntax)
            {
                propertyName = ((IdentifierNameSyntax)node).ToString();
            }
            else if (node is MemberAccessExpressionSyntax)
            {
                target = visitor.Visit(((MemberAccessExpressionSyntax)node).Expression);
                propertyName = ((MemberAccessExpressionSyntax)node).Name.ToString();
            }

            if (propertyName != null && !visitor.IsSpecialPropertyName(propertyName))
            {
                var propertyInfo = target.GetType().GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    return propertyInfo;
                }
            }
            throw new NotSupportedException(string.Format("Cannot update the source of the binding '{0}'!", expression.Expression));
        }

        /// <summary>
        /// Gets the root data context for a specified control.
        /// </summary>
        private object GetRootDataContext(DotvvmControl viewRoot)
        {
            if (viewRoot is DotvvmBindableControl)
            {
                return ((DotvvmBindableControl)viewRoot).DataContext;
            }
            throw new Exception("The view root must be bindable control!");     // TODO: exception handling
        }
    }
}
