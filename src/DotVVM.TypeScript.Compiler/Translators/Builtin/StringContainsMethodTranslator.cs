using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.Factories;
using Microsoft.CodeAnalysis.Operations;

namespace DotVVM.TypeScript.Compiler.Translators.Builtin
{
    class StringContainsMethodTranslator : IMethodCallTranslator
    {
        private readonly ISyntaxFactory _factory;

        public StringContainsMethodTranslator(ISyntaxFactory factory)
        {
            _factory = factory;
        }

        public ISyntaxNode Translate(IInvocationOperation operation, List<IExpressionSyntax> arguments, IReferenceSyntax reference, ISyntaxNode parent)
        {
            var format = $"({reference.ToDisplayString()}.indexOf({arguments.Single()}) !== -1)";
            return _factory.CreateRawSyntaxNode(format, parent);
        }
    }
}
