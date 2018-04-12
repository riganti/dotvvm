using System.Collections.Generic;
using DotVVM.TypeScript.Compiler.Ast;
using Microsoft.CodeAnalysis.Operations;

namespace DotVVM.TypeScript.Compiler.Translators
{
    public interface IMethodCallTranslator
    {
        ISyntaxNode Translate(IInvocationOperation operation, List<IExpressionSyntax> arguments, IReferenceSyntax reference, ISyntaxNode parent);
    }
}