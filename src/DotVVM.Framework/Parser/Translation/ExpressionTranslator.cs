using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Parser.Translation
{
    /// <summary>
    /// A translator that takes C# expressions and translates them to the TypeScript
    /// </summary>
    public class ExpressionTranslator
    {


        /// <summary>
        /// Translates the specified expression.
        /// </summary>
        public string Translate(string expression)
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
            var visitor = new ExpressionTranslatorVisitor();
            var result = visitor.Visit(node);

            if (!visitor.IsExpression)
            {
                if (result.EndsWith("()"))
                {
                    return result.Substring(0, result.Length - 2);
                }
            }
            return result;
        }


    }
}
