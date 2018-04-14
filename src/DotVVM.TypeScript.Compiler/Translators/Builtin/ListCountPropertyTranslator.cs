using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.Factories;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators.Builtin
{
    public class ListCountPropertyTranslator : IPropertyTranslator
    {

        private readonly ISyntaxFactory _factory;

        public ListCountPropertyTranslator(ISyntaxFactory factory)
        {
            _factory = factory;
        }

        public ISyntaxNode Translate(IReferenceSyntax instanceReference, IPropertySymbol property, ISyntaxNode parent)
        {
            var access = "";
            if (instanceReference is IPropertyReferenceSyntax propertyReference)
            {
                access = propertyReference.Instance.ToDisplayString() + ".";
            }
            access += instanceReference.Identifier.ToDisplayString();
            var format = $"{access}().length";
            return _factory.CreateRawSyntaxNode(format, parent);
        }
    }
}
