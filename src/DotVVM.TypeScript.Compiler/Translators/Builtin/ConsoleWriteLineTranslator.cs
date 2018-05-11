using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.Factories;
using Microsoft.CodeAnalysis.Operations;

namespace DotVVM.TypeScript.Compiler.Translators.Builtin
{
    public class ConsoleWriteLineTranslator : IMethodCallTranslator
    {
        private readonly ISyntaxFactory _factory;

        public ConsoleWriteLineTranslator(ISyntaxFactory factory)
        {
            _factory = factory;
        }

        public ISyntaxNode Translate(IInvocationOperation operation, List<IExpressionSyntax> arguments, IReferenceSyntax reference, ISyntaxNode parent)
        {
            return _factory.CreateRawSyntaxNode($"console.log({arguments.Select(a => a.ToDisplayString()).StringJoin(",")});", parent);
        }
    }
}