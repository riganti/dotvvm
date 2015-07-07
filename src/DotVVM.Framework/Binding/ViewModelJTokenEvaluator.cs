using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;
using DotVVM.Framework.Parser;

namespace DotVVM.Framework.Binding
{
    public class ViewModelJTokenEvaluator : IExpressionEvaluator<JToken>
    {

        private ViewModelJTokenEvaluationVisitor visitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelJTokenEvaluator"/> class.
        /// </summary>
        public ViewModelJTokenEvaluator(JToken root)
        {
            visitor = new ViewModelJTokenEvaluationVisitor(root);
        }

        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        public JToken Evaluate(string expression)
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
            return visitor.Visit(node);
        }

    }
}
