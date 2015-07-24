using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DotVVM.Framework.Parser;

namespace DotVVM.Framework.Runtime.Compilation
{
    public static class RoslynExtensionMethods
    {

        public static CompilationUnitSyntax WithMembers(this CompilationUnitSyntax syntax, params MemberDeclarationSyntax[] members)
        {
            return syntax.WithMembers(SyntaxFactory.List(members));
        }

        public static NamespaceDeclarationSyntax WithMembers(this NamespaceDeclarationSyntax syntax, params MemberDeclarationSyntax[] members)
        {
            return syntax.WithMembers(SyntaxFactory.List(members));
        }

        public static ClassDeclarationSyntax WithMembers(this ClassDeclarationSyntax syntax, params MemberDeclarationSyntax[] members)
        {
            return syntax.WithMembers(SyntaxFactory.List(members));
        }

        public static VariableDeclarationSyntax WithVariables(this VariableDeclarationSyntax syntax, params VariableDeclaratorSyntax[] variables)
        {
            return syntax.WithVariables(SyntaxFactory.SeparatedList(variables));
        }

        public static ExpressionStatementSyntax EnsureSingleExpression(this SyntaxTree tree)
        {
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
            return expr;
        }
    }
}
