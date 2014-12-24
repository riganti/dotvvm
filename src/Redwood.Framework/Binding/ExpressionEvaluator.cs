using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;
using Redwood.Framework.Parser;

namespace Redwood.Framework.Binding
{
    public class ExpressionEvaluator
    {

        public bool AllowMethods { get; set; }


        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        public object Evaluate(string expression, object dataContext)
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

            // translate the expression
            var visitor = new ExpressionEvaluationVisitor { AllowMethods = AllowMethods, DataContext = dataContext, Root = dataContext };
            return visitor.Visit(node);
        }

    }
}
