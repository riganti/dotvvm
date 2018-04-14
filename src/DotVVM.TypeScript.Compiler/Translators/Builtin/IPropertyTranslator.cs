using DotVVM.TypeScript.Compiler.Ast;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators.Builtin
{
    interface IPropertyTranslator
    {
        ISyntaxNode Translate(IReferenceSyntax instanceReference, IPropertySymbol property, ISyntaxNode parent);
    }
}
