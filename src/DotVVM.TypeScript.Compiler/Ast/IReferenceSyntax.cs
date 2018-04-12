using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IReferenceSyntax : IExpressionSyntax
    {
        IIdentifierSyntax Identifier { get; }
    }
}
