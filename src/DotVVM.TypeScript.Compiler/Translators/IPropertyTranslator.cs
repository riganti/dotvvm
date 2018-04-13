using DotVVM.TypeScript.Compiler.Ast;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators
{
    interface IPropertyTranslator
    {
        ISyntaxNode Translate(IReferenceSyntax instanceReference, IPropertySymbol property);
    }
}