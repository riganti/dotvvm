using System.Collections.Immutable;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IMethodCallSyntax : IExpressionSyntax
    {
        IReferenceSyntax Object { get; set; }
        IIdentifierSyntax Name { get; }
        ImmutableList<IExpressionSyntax> Arguments { get; }
        void SetArguments(ImmutableList<IExpressionSyntax> arguments);
    }
}
