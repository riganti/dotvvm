using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Redwood.Framework.Parser.Translation
{
    /// <summary>
    /// A translator that takes C# expressions and translates them to the TypeScript
    /// </summary>
    public class ExpressionTranslator
    {

        private SyntaxNode ParseExpression(string expression)
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
            return expr.ChildNodes().First();
        }

        /// <summary>
        /// Translates the specified expression.
        /// </summary>
        public string Translate(string expression)
        {
            var node = ParseExpression(expression);

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

        public string[] TranslateToPath(string expression)
        {
            var node = ParseExpression(expression);

            // translate the expression
            var visitor = new ExpressionTranslatorVisitor();
            var result = visitor.Visit(node);

            if (visitor.IsExpression)
            {
                return new string[] { "`" + result + "`" };
            }
            else
            {
                return visitor.Path.ToArray();
            }
        }

        public static string[] CombinePaths(IEnumerable<string> a, IEnumerable<string> b)
        {
            return SimplifyPath(a.Concat(b));
        }

        public static string[] SimplifyPath(IEnumerable<string> path)
        {
            var result = new Stack<string>();
            foreach (var frag in path)
            {
                if (frag == "$root")
                {
                    result.Clear();
                    result.Push(frag);
                }
                else if (frag == "$parent")
                {
                    if (result.Count > 0)
                        result.Pop();
                    else result.Push("$parent");
                }
                else if (frag == "$data") { }
                else
                {
                    result.Push(frag);
                }
            }
            return result.Reverse().ToArray();
        }
    }
}
