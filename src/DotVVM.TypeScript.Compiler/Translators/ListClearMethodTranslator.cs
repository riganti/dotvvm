using System;
using System.Collections.Generic;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.Factories;
using Microsoft.CodeAnalysis.Operations;

namespace DotVVM.TypeScript.Compiler.Translators
{
    public class ListClearMethodTranslator : IMethodCallTranslator
    {
        private readonly ISyntaxFactory _factory;

        public ListClearMethodTranslator(ISyntaxFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }


        public ISyntaxNode Translate(IInvocationOperation operation, List<IExpressionSyntax> arguments, IReferenceSyntax reference, ISyntaxNode parent)
        {
            var format = $"{reference.ToDisplayString()}.removeAll()";
            return _factory.CreateRawSyntaxNode(format, parent);
        }
    }
}