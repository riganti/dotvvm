using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Redwood.Framework.Parser;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Binding
{
    /// <summary>
    /// Finds the command to execute in the viewmodel using the path and command expression.
    /// </summary>
    public class CommandResolver
    {

        public Action GetFunction(object viewModel, string[] path, string command)
        {
            object pathObject = viewModel;
            List<object> hierarchy;

            // resolve the path
            var pathExpression = GetFullCommandPath(path);
            if (!string.IsNullOrEmpty(pathExpression))
            {
                var pathTree = CSharpSyntaxTree.ParseText(pathExpression, new CSharpParseOptions(LanguageVersion.CSharp5, DocumentationMode.Parse, SourceCodeKind.Interactive));
                var pathExpr = pathTree.EnsureSingleExpression();

                // find the target on which the function is called
                var pathExpressionEvaluator = new ExpressionEvaluationVisitor { Root = viewModel, DataContext = viewModel };
                pathObject = pathExpressionEvaluator.Visit(pathExpr);
                hierarchy = pathExpressionEvaluator.Hierarchy.Reverse().ToList();
            }
            else
            {
                hierarchy = new List<object>();
            }

            // find the function on that path
            var tree = CSharpSyntaxTree.ParseText(command, new CSharpParseOptions(LanguageVersion.CSharp5, DocumentationMode.Parse, SourceCodeKind.Interactive));
            var expr = tree.EnsureSingleExpression();
            var node = expr.ChildNodes().First() as InvocationExpressionSyntax;
            if (node == null)
            {
                throw new ParserException("The expression in command must be a method call!");
            }
            var methodEvaluator = new ExpressionEvaluationVisitor()
            {
                Root = viewModel,
                DataContext = pathObject,
                AllowMethods = true,
                Hierarchy = new Stack<object>(hierarchy)
            };
            var method = methodEvaluator.Visit(node.Expression) as MethodInfo;
            if (method == null)
            {
                throw new Exception("The path was not found!");
            }


            // parse arguments
            var arguments = node.ArgumentList.Arguments.Select(a =>
            {
                var evaluator = new ExpressionEvaluationVisitor()
                {
                    Root = viewModel,
                    DataContext = pathObject,
                    Hierarchy = new Stack<object>(hierarchy)
                };
                return evaluator.Visit(a);
            }).ToArray();

            // return the delegate for further invoke
            return () => method.Invoke(methodEvaluator.Target, arguments);
        }


        /// <summary>
        /// Gets the full command path.
        /// </summary>
        private string GetFullCommandPath(IEnumerable<string> path)
        {
            var pathString = new StringBuilder();
            foreach (var fragment in path)
            {
                if (pathString.Length > 0 && !fragment.StartsWith("["))
                {
                    pathString.Append(".");
                }
                pathString.Append(fragment);
            }
            return pathString.ToString();
        }
    }
}