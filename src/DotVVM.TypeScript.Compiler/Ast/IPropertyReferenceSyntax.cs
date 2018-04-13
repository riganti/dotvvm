using DotVVM.TypeScript.Compiler.Ast.TypeScript;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IPropertyReferenceSyntax : IReferenceSyntax
    {
        IReferenceSyntax Instance { get; }
        ITypeSymbol  Type { get; }
    }
}
