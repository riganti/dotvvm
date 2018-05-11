using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IVariableDeclaratorSyntax : IExpressionSyntax
    {
        IExpressionSyntax Expression { get; }
        IIdentifierSyntax Identifier { get; }
    }
}
