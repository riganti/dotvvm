using System.Collections.Generic;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface INamespaceDeclarationSyntax : ISyntaxNode
    {
        IIdentifierSyntax Identifier { get; }
        IList<IMemberDeclarationSyntax> Types { get; }
    }
}
