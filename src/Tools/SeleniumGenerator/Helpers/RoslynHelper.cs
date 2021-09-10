using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Helpers
{
    public static class RoslynHelper
    {
        public static ObjectCreationExpressionSyntax GetPathSelectorObjectInitialization(string propertyName)
        {
            return SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.IdentifierName("PathSelector"))
                .WithInitializer(
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.ObjectInitializerExpression,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(new SyntaxNodeOrToken[]
                        {
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName("UiName"),
                                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(propertyName))),
                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName("Parent"),
                                SyntaxFactory.IdentifierName("parentSelector"))
                        })
                    )
                );
        }

        public static TypeSyntax ParseTypeName(string typeName, params string[] genericTypeNames)
        {
            if (genericTypeNames.Length == 0)
            {
                return SyntaxFactory.ParseTypeName(typeName);
            }

            return SyntaxFactory.GenericName(typeName)
                .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(
                    genericTypeNames.Select(n => SyntaxFactory.ParseTypeName(n)))
                ));
        }
    }
}
