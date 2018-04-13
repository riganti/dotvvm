using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.Factories;
using Microsoft.CodeAnalysis.Operations;

namespace DotVVM.TypeScript.Compiler.Translators
{
    public class ListAddMethodTranslator : IMethodCallTranslator
    {
        private readonly ISyntaxFactory _factory;

        public ListAddMethodTranslator(ISyntaxFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }


        public ISyntaxNode Translate(IInvocationOperation operation, List<IExpressionSyntax> arguments, IReferenceSyntax reference, ISyntaxNode parent)
        {
            var format = $"{reference.ToDisplayString()}.push(ko.observable({arguments.Single().ToDisplayString()}))";
            return _factory.CreateParametrizedSyntaxNode(format, parent);
        }
    }
}