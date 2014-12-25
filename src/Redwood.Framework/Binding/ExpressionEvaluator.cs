using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Redwood.Framework.Controls;
using Redwood.Framework.Parser;

namespace Redwood.Framework.Binding
{
    public class ExpressionEvaluator
    {

        public bool AllowMethods { get; set; }


        
        /// <summary>
        /// Evaluates the specified expression in the context of specified control.
        /// </summary>
        public object Evaluate(ValueBindingExpression expression, RedwoodProperty property, RedwoodBindableControl contextControl)
        {
            // get the hierarchy of DataContext the control is in
            var dataContexts = contextControl.GetAllAncestors().OfType<RedwoodBindableControl>()
                .Select(c => new { Binding = c.GetBinding(RedwoodBindableControl.DataContextProperty), Control = c })
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
                    throw new Exception("The DataContext property can only contain value bindings!");     // TODO: exception handling
                }
                var pathExpression = ((ValueBindingExpression)binding).GetViewModelPathExpression(dataContexts[i].Control, RedwoodBindableControl.DataContextProperty);
                EvaluateBinding(visitor, binding.Expression, pathExpression);
            }

            // evaluate the final expression
            EvaluateBinding(visitor, expression.Expression, expression.GetViewModelPathExpression(contextControl, property));
            return visitor.Result;
        }

        /// <summary>
        /// Evaluates the binding.
        /// </summary>
        internal object EvaluateBinding(ExpressionEvaluationVisitor visitor, string expression, string pathExpression)
        {
            // parse
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

            var result = visitor.Visit(node);
            visitor.BackupCurrentPosition(result, pathExpression);
            return result;
        }

        /// <summary>
        /// Gets the root data context for a specified control.
        /// </summary>
        private object GetRootDataContext(RedwoodControl viewRoot)
        {
            if (viewRoot is RedwoodBindableControl)
            {
                return ((RedwoodBindableControl)viewRoot).DataContext;
            }
            throw new Exception("The view root must be bindable control!");     // TODO: exception handling
        }
        
    }
}
