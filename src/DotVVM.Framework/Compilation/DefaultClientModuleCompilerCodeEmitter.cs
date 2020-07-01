using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Compilation
{
    public class DefaultClientModuleCompilerCodeEmitter : CodeEmitterBase
    {
        /// <summary>
        /// Gets the result syntax tree.
        /// </summary>
        public IEnumerable<SyntaxTree> BuildTree(string namespaceName, string className, IEnumerable<UsingDirectiveSyntax> usings)
        {
            var root = SyntaxFactory.CompilationUnit()
                .WithExterns(SyntaxFactory.List(
                    UsedAssemblies.Select(k => SyntaxFactory.ExternAliasDirective(SyntaxFactory.Identifier(k.Identifier)))
                ))
                .WithUsings(SyntaxFactory.List(usings))
                .WithMembers(
                    SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceName))
                        .WithMembers(
                            SyntaxFactory.List<MemberDeclarationSyntax>(new[]
                            {
                                SyntaxFactory.ClassDeclaration(className)
                                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                    .WithMembers(SyntaxFactory.List(otherDeclarations))
                            })
                        )
                );

            return new[] { root.SyntaxTree };
        }

        public void AddMembers(List<MemberDeclarationSyntax> members)
        {
            otherDeclarations.AddRange(members);
        }

    }
}
