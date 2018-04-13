using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.Factories;
using Microsoft.CodeAnalysis.Operations;

namespace DotVVM.TypeScript.Compiler.Translators.Builtin
{
    public class ListRemoveMethodTranslator : IMethodCallTranslator
    {
        private readonly ISyntaxFactory _factory;

        public ListRemoveMethodTranslator(ISyntaxFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public ISyntaxNode Translate(IInvocationOperation operation, List<IExpressionSyntax> arguments, IReferenceSyntax reference, ISyntaxNode parent)
        {
            var format = $"{reference.ToDisplayString()}.remove(function (item) {{ var rawItem = ko.unwrap(item); return rawItem == {arguments.Single().ToDisplayString()};}})";
            return _factory.CreateRawSyntaxNode(format, parent);
        }
    }
}
