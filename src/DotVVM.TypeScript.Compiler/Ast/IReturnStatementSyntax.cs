using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IReturnStatementSyntax : IStatementSyntax
    {
        IExpressionSyntax Expression { get; }
    }
}
