using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface ITypeSyntax : ISyntaxNode
    {
        ITypeSymbol EquivalentSymbol { get; }
    }
}